import 'cypress-wait-until';

describe('Sistema de Gestão de Lugares (Seats)', () => {

  const CINEMA_ID = 1;
  const HALL_ID = 999;

  const mockCinema = {
    id: CINEMA_ID,
    name: 'Cinema Teste',
    city: 'Lisboa'
  };

  const mockSeatsForEdit = [
    { id: 1, seatNumber: 'A1', seatType: 'Reduced' },
    { id: 2, seatNumber: 'A2', seatType: 'Reduced' },
    { id: 3, seatNumber: 'B1', seatType: 'Normal' },
    { id: 4, seatNumber: 'B2', seatType: 'VIP' }
  ];

  beforeEach(() => {
    cy.loginMock();

    cy.intercept('POST', '**/User/refresh', {
      statusCode: 200,
      body: { accessToken: 'fake-new', refreshToken: 'fake-new' }
    });
  });

  function getHallFormComponent() {
    return cy.get('app-hall-form', { timeout: 8000 }).then($el =>
      cy.window().then(win => {
        const ng = (win as any).ng;
        if (!ng || !ng.getComponent) {
          throw new Error('Angular component API not available (win.ng.getComponent undefined)');
        }

        return {
          cmp: ng.getComponent($el[0]),
          win
        };
      })
    );
  }

  function forceSubmit() {
    return getHallFormComponent().then(({ cmp, win }) => {
      (win as any).Zone.current.run(() => {
        cmp.submit();
      });
    });
  }

  function generateMockSeats(rows = 10, cols = 5) {
    const seatTypes = [];
    const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';

    let idCounter = 1;

    for (let r = 0; r < rows; r++) {
      const rowLetter = alphabet[r];

      for (let c = 1; c <= cols; c++) {
        seatTypes.push({
          id: idCounter++,
          seatNumber: `${rowLetter}${c}`,
          seatType: r === 0 ? 'Reduced' : 'Normal'
        });
      }
    }

    return seatTypes;
  }

  it('Deve gerar o mapa com fila A fixa (Reduced) e restantes Normal', () => {

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, {
      statusCode: 200,
      body: mockCinema
    }).as('getCinema');

    cy.visit(`/cinemas/${CINEMA_ID}/halls/create`);

    cy.wait('@getCinema');

    cy.get('#hall-name').type('Sala Teste');
    cy.get('#hall-rows').clear().type('3');
    cy.get('#hall-seats').clear().type('4');

    cy.get('.seat').should('have.length', 12);

    cy.get('.seat-row').first().find('.seat')
      .should('have.class', 'is-reduced');

    cy.get('.seat-row').eq(1).find('.seat')
      .should('have.class', 'is-normal');
  });

  it('Deve alternar Normal ↔ VIP e impedir alterar Reduced', () => {

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, { body: mockCinema });

    cy.visit(`/cinemas/${CINEMA_ID}/halls/create`);

    cy.get('#hall-rows').clear().type('2');
    cy.get('#hall-seats').clear().type('3');

    cy.get('.seat-row').first().find('.seat').first().click();
    cy.get('.seat-row').first().find('.seat').first()
      .should('have.class', 'is-reduced');

    const seat = cy.get('.seat-row').eq(1).find('.seat').first();

    seat.click().should('have.class', 'is-vip');
    seat.click().should('have.class', 'is-normal');
  });

  it('Deve atualizar estatísticas automaticamente', () => {

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, { body: mockCinema });

    cy.visit(`/cinemas/${CINEMA_ID}/halls/create`);

    cy.get('#hall-rows').clear().type('2');
    cy.get('#hall-seats').clear().type('2');

    cy.contains('Normal: 2');
    cy.contains('VIP: 0');
    cy.contains('Reduzida: 2');

    cy.get('.seat-row').eq(1).find('.seat').first().click();

    cy.contains('VIP: 1');
    cy.contains('Normal: 1');
  });

  it('Deve criar sala e gerar lugares (payload correto)', () => {

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, { body: mockCinema });

    cy.intercept('POST', '**/CinemaHall', {
      statusCode: 201,
      body: { id: HALL_ID }
    }).as('createHall');

    const createdSeats: any[] = [];

    cy.intercept('POST', '**/Seat', req => {
      createdSeats.push(req.body);
      req.reply({ statusCode: 201 });
    }).as('addSeat');

    cy.visit(`/cinemas/${CINEMA_ID}/halls/create`);

    cy.get('#hall-name').type('Sala Nova');
    cy.get('#hall-rows').clear().type('2');
    cy.get('#hall-seats').clear().type('2');

    forceSubmit();

    cy.wait('@createHall');

    cy.waitUntil(() => createdSeats.length === 4);

    cy.wrap(createdSeats).then(list => {
      expect(list).to.deep.include({
        cinemaHallId: HALL_ID,
        seatNumber: 'A1',
        seatType: 'Reduced'
      });
      expect(list).to.deep.include({
        cinemaHallId: HALL_ID,
        seatNumber: 'B2',
        seatType: 'Normal'
      });
    });
  });

  it('Deve carregar sala existente e reconstruir o mapa visual a partir dos lugares da BD', () => {

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, { body: mockCinema });

    cy.intercept('GET', `**/CinemaHall/${HALL_ID}`, {
      statusCode: 200,
      body: { id: HALL_ID, name: 'Sala Existente', cinemaId: CINEMA_ID }
    }).as('getHall');

    cy.intercept('GET', `**/Seat/hall/${HALL_ID}`, {
      statusCode: 200,
      body: mockSeatsForEdit
    }).as('getSeats');

    cy.visit(`/cinemas/${CINEMA_ID}/halls/${HALL_ID}/edit`);

    cy.wait('@getHall');
    cy.wait('@getSeats');

    cy.get('.seat').should('have.length', 4);

    cy.contains('.row-label', 'A').closest('.seat-row')
      .parent()
      .find('.seat')
      .first()
      .should('have.class', 'is-reduced');

    cy.contains('B')
      .parent()
      .find('.seat')
      .eq(1)
      .should('have.class', 'is-vip');
  });

  it('Deve criar sala 10x5 e selecionar aleatoriamente 10 lugares como VIP', () => {

    const rows = 10;
    const cols = 5;
    const totalSeats = rows * cols;

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, { body: mockCinema });

    cy.intercept('POST', '**/CinemaHall', {
      statusCode: 201,
      body: { id: HALL_ID }
    }).as('createHall');

    const createdSeats: any[] = [];

    cy.intercept('POST', '**/Seat', req => {
      createdSeats.push(req.body);
      req.reply({ statusCode: 201 });
    }).as('addSeat');

    cy.visit(`/cinemas/${CINEMA_ID}/halls/create`);

    cy.get('#hall-name').type('Sala 10x5 Automática');
    cy.get('#hall-rows').clear().type(rows.toString());
    cy.get('#hall-seats').clear().type(cols.toString());

    cy.get('.seat').should('have.length', totalSeats);

    cy.contains('.row-label', 'A')
      .closest('.seat-row')
      .find('.seat')
      .each($s => cy.wrap($s).should('have.class', 'is-reduced'));

    const vipIndexes: number[] = [];

    while (vipIndexes.length < 10) {
      const index = Math.floor(Math.random() * totalSeats);
      if (index < cols) continue;
      if (!vipIndexes.includes(index)) {
        vipIndexes.push(index);
      }
    }

    cy.get('.seat').then($seats => {
      vipIndexes.forEach(i => {
        cy.wrap($seats[i])
          .click()
          .should('have.class', 'is-vip');
      });
    });

    forceSubmit();

    cy.wait('@createHall');

    cy.waitUntil(() => createdSeats.length === totalSeats);

    cy.wrap(createdSeats).then(list => {
      vipIndexes.forEach(index => {
        const seat = list[index];
        expect(seat.seatType).to.eq('VIP');
      });
    });
  });

  it('Deve apagar seats antigos e recriar todos', () => {

    cy.intercept('GET', `**/Cinemas/${CINEMA_ID}`, { body: mockCinema });

    cy.intercept('GET', `**/CinemaHall/${HALL_ID}`, {
      body: { id: HALL_ID, name: 'Sala Edit', cinemaId: CINEMA_ID }
    });

    cy.intercept('GET', `**/Seat/hall/${HALL_ID}`, { body: mockSeatsForEdit });

    cy.intercept('PUT', `**/CinemaHall/${HALL_ID}`, {
      statusCode: 200
    }).as('updateHall');

    const deletedSeats: number[] = [];
    cy.intercept('DELETE', '**/Seat/*', req => {
      deletedSeats.push(Number(req.url.split('/').pop()));
      req.reply({ statusCode: 200 });
    }).as('deleteSeat');

    const newSeats: any[] = [];
    cy.intercept('POST', '**/Seat', req => {
      newSeats.push(req.body);
      req.reply({ statusCode: 201 });
    }).as('addSeat');

    cy.visit(`/cinemas/${CINEMA_ID}/halls/${HALL_ID}/edit`);

    cy.get('#hall-name').clear().type('Sala Editada');

    forceSubmit();

    cy.wait('@updateHall');

    cy.waitUntil(() => deletedSeats.length === mockSeatsForEdit.length);
    cy.waitUntil(() => newSeats.length === mockSeatsForEdit.length);
  });

});
