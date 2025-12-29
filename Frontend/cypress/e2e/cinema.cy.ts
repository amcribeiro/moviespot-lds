describe('Sistema de Gestão de Cinemas', () => {

  const mockCinemas = [
    {
      id: 1,
      name: 'Cinema NOS',
      city: 'Lisboa',
      street: 'Av. Liberdade',
      country: 'PT',
      latitude: 0,
      longitude: 0
    },
    {
      id: 2,
      name: 'Cinema UCI',
      city: 'Porto',
      street: 'Arrábida',
      country: 'PT',
      latitude: 0,
      longitude: 0
    }
  ];

  const mockHalls = [
    { id: 10, name: 'Sala 1 IMAX', cinemaId: 1 },
    { id: 11, name: 'Sala 2 VIP', cinemaId: 1 }
  ];

  const mockSeats = [
    { id: 101, row: 'A', number: 1, seatType: 'VIP' },
    { id: 102, row: 'A', number: 2, seatType: 'Normal' }
  ];

  beforeEach(() => {

    cy.loginMock();

    cy.intercept('POST', '**/User/refresh', {
      statusCode: 200,
      body: {
        accessToken: 'new-fake',
        refreshToken: 'new-fake'
      }
    }).as('refreshReq');

    cy.intercept('GET', '**/Cinemas', {
      statusCode: 200,
      body: mockCinemas
    }).as('getCinemas');

  });

  it('Deve listar cinemas e expandir salas (Accordion)', () => {

    cy.intercept('GET', '**/CinemaHall/cinema/*', {
      statusCode: 200,
      body: mockHalls
    }).as('getHalls');

    cy.intercept('GET', '**/Seat/hall/*', {
      statusCode: 200,
      body: mockSeats
    }).as('getSeats');

    cy.visit('/cinemas');
    cy.wait('@getCinemas');

    cy.contains('Cinema NOS').click();
    cy.wait('@getHalls');

    cy.contains('Sala 1 IMAX').should('be.visible');

    cy.get('.inner-table tbody tr')
      .first()
      .find('td')
      .eq(1)
      .should('contain', '2');

  });

  it('Deve apagar uma sala com os seus lugares', () => {

    cy.intercept('GET', '**/CinemaHall/cinema/*', { body: mockHalls });
    cy.intercept('GET', '**/Seat/hall/*', { body: mockSeats });

    cy.intercept('GET', '**/Seat/hall/10', { body: mockSeats }).as('fetchSeats');
    cy.intercept('DELETE', '**/Seat/*', { statusCode: 200 }).as('deleteSeat');
    cy.intercept('DELETE', '**/CinemaHall/*', { statusCode: 200 }).as('deleteHall');

    cy.visit('/cinemas');

    cy.contains('Cinema NOS').click();

    cy.on('window:confirm', () => true);

    cy.contains('Sala 1 IMAX')
      .closest('tr')
      .find('.btn-small.red')
      .click();

    cy.wait('@fetchSeats');
    cy.wait('@deleteSeat');
    cy.wait('@deleteSeat');
    cy.wait('@deleteHall');

    cy.contains('Sala 1 IMAX').should('not.exist');

  });

  it('Deve validar form vazio, preencher mapa e criar cinema', () => {

    cy.visit('/cinemas/create');

    cy.contains('Criar Cinema').should('be.visible');

    cy.intercept('POST', '**/Cinemas', {
      statusCode: 400,
      body: { error: 'Dados inválidos' }
    }).as('createFail');

    cy.get('.btn-create').click({ force: true });
    cy.wait('@createFail');

    cy.get('#cinema-name')
      .should('have.class', 'ng-invalid');

    cy.intercept('POST', '**/Cinemas', {
      statusCode: 201,
      body: { id: 99 }
    }).as('createSuccess');

    cy.get('#cinema-name')
      .type('Cinema Do Cypress');

    cy.simulateMapSelection(38.707, -9.136, {
      road: 'Praça do Comércio',
      city: 'Lisboa',
      state: 'Lisboa',
      postcode: '1100-001',
      country: 'Portugal'
    });

    cy.get('app-cinema-form').then($el => {

      cy.window().then(win => {

        const component = (win as any).ng.getComponent($el[0]);

        expect(component.form.city).to.eq('Lisboa');
        expect(component.form.state).to.eq('Lisboa');
        expect(component.form.zipCode).to.eq('1100-001');
        expect(component.form.country).to.eq('Portugal');

      });

    });

    cy.get('.btn-create').click({ force: true });

    cy.wait('@createSuccess');

    cy.url().should('include', '/cinemas');

  });

  it('Deve carregar dados, editar e guardar alterações', () => {

    const cinema = {
      id: 1,
      name: 'Cinema NOS',
      street: 'Av. Liberdade',
      city: 'Lisboa',
      state: 'Lisboa',
      zipCode: '1000-001',
      country: 'Portugal',
      latitude: 38.7,
      longitude: -9.13
    };

    cy.intercept('GET', '**/Cinemas/1', {
      statusCode: 200,
      body: cinema
    });

    cy.intercept('PUT', '**/Cinemas/1', {
      statusCode: 200,
      body: {}
    }).as('updateCinema');

    cy.visit('/cinemas/edit/1');

    cy.contains('Editar Cinema').should('be.visible');

    cy.get('#cinema-name')
      .should('have.value', 'Cinema NOS')
      .clear()
      .type('Cinema Atualizado Cypress');

    cy.get('.btn-create').click({ force: true });

    cy.wait('@updateCinema');

    cy.url().should('include', '/cinemas');

  });

  it('Deve apagar um cinema da listagem', () => {

    cy.intercept('DELETE', '**/Cinemas/1', {
      statusCode: 200
    }).as('deleteCinema');

    cy.visit('/cinemas');

    cy.wait('@getCinemas');

    cy.on('window:confirm', () => true);

    cy.contains('Cinema NOS')
      .closest('tr')
      .find('.btn-small.red')
      .click();

    cy.wait('@deleteCinema');

    cy.contains('Cinema NOS').should('not.exist');

  });

  it('Deve mostrar mensagem quando criação falha', () => {

    cy.visit('/cinemas/create');

    cy.contains('Criar Cinema').should('be.visible');

    cy.intercept('POST', '**/Cinemas', {
      statusCode: 500,
      body: { error: 'Erro servidor' }
    }).as('createError');

    cy.get('#cinema-name')
      .type('Cinema Erro');

    cy.simulateMapSelection(38.7, -9.13, {
      road: 'Rua X',
      city: 'Lisboa',
      state: 'Lisboa',
      postcode: '1000-000',
      country: 'Portugal'
    });

    cy.get('.btn-create').click({ force: true });

    cy.wait('@createError');

    cy.contains('Erro ao criar cinema').should('be.visible');

    cy.url().should('include', '/cinemas/create');

  });

});
