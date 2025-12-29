
import 'cypress-wait-until';
describe('Sistema de Gestão de Sessões', () => {


  const mockMovies = [
    { id: 1, title: 'Duna', duration: 150 },
    { id: 2, title: 'Oppenheimer', duration: 180 },
  ];

  const mockCinemas = [
    {
      id: 1,
      name: 'Cinema NOS Colombo',
      city: 'Lisboa',
      street: 'Av. X',
      country: 'PT',
      latitude: 0,
      longitude: 0,
    },
  ];

  const mockHalls = [
    { id: 10, name: 'Sala IMAX', cinemaId: 1 },
    { id: 11, name: 'Sala VIP', cinemaId: 1 },
  ];

  const mockSeatsHall10 = [
    { id: 1, row: 'A', number: 1, seatType: 'Normal' },
    { id: 2, row: 'A', number: 2, seatType: 'VIP' },
  ];

  const mockSessions = [
    {
      id: 1,
      movieId: 1,
      movieTitle: 'Duna',
      cinemaHallId: 10,
      cinemaHallName: 'Sala IMAX',
      createdBy: 1,
      createdByName: 'Admin',
      startDate: '2099-12-31T21:00:00Z',
      endDate: '2100-01-01T00:00:00Z',
      price: 9.5,
      createdAt: '2099-01-01T00:00:00Z',
      updatedAt: '2099-01-01T00:00:00Z',
    },
    {
      id: 2,
      movieId: 2,
      movieTitle: 'Oppenheimer',
      cinemaHallId: 11,
      cinemaHallName: 'Sala VIP',
      createdBy: 2,
      createdByName: 'Staff',
      startDate: '2099-12-31T18:00:00Z',
      endDate: '2099-12-31T21:00:00Z',
      price: 10,
      createdAt: '2099-01-01T00:00:00Z',
      updatedAt: '2099-01-01T00:00:00Z',
    },
  ];

  const availableTimes = ['18:00', '21:00'];

  beforeEach(() => {
    cy.loginMock();

    cy.intercept('POST', '**/User/refresh', {
      statusCode: 200,
      body: {
        accessToken: 'new-fake',
        refreshToken: 'new-fake',
      },
    }).as('refreshReq');
  });

  function interceptListSessions(sessions = mockSessions) {
    cy.intercept('GET', '**/Session', {
      statusCode: 200,
      body: sessions,
    }).as('getSessions');
  }

  function interceptMovieAndCinemaData() {
    cy.intercept('GET', '**/Movie', {
      statusCode: 200,
      body: mockMovies,
    }).as('getMovies');

    cy.intercept('GET', '**/Cinemas', {
      statusCode: 200,
      body: mockCinemas,
    }).as('getCinemas');
  }

  function interceptHallsAndSeats() {
    cy.intercept('GET', '**/CinemaHall/cinema/1', {
      statusCode: 200,
      body: mockHalls,
    }).as('getHallsByCinema');

    cy.intercept('GET', '**/Seat/hall/10', {
      statusCode: 200,
      body: mockSeatsHall10,
    }).as('getSeatsHall10');

    cy.intercept('GET', '**/Seat/hall/11', {
      statusCode: 200,
      body: [],
    }).as('getSeatsHall11');
  }

  function interceptAvailableTimes(
    times: string[] = availableTimes,
    statusCode = 200
  ) {
    cy.intercept('GET', '**/Session/available-times*', req => {
      req.reply({
        statusCode,
        body: statusCode === 200
          ? times
          : { message: 'Erro ao carregar horários.' },
      });
    }).as('getAvailableTimes');
  }

  function getSessionFormComponent() {
    return cy.get('app-session-form').then($el =>
      cy.window().then(win => ({
        cmp: (win as any).ng.getComponent($el[0]),
        win,
      }))
    );
  }

  function forceLoadAvailableTimes() {
    return getSessionFormComponent().then(({ cmp, win }) => {
      (win as any).Zone.current.run(() => {
        cmp.loadAvailableTimes();
      });
    });
  }

  it('Deve listar sessões com os dados principais', () => {

    interceptListSessions();

    cy.visit('/sessions');

    cy.wait('@getSessions');

    cy.contains('Sessões').should('be.visible');

    cy.contains('Duna');
    cy.contains('Oppenheimer');
    cy.contains('Sala IMAX');
    cy.contains('Sala VIP');

    cy.get('table tbody tr')
      .should('have.length', 2);
  });

  it('Deve mostrar mensagem quando não existirem sessões', () => {

    interceptListSessions([]);

    cy.visit('/sessions');

    cy.wait('@getSessions');

    cy.contains('Ainda não existem sessões criadas')
      .should('be.visible');
  });

  it('Deve criar uma sessão com sucesso', () => {

    interceptMovieAndCinemaData();
    interceptHallsAndSeats();
    interceptAvailableTimes();

    cy.intercept('GET', '**/Session', {
      statusCode: 200,
      body: [{
        id: 123,
        movieTitle: 'Duna',
        cinemaHallName: 'Sala IMAX',
        createdByName: 'Admin',
        price: 8.5
      }]
    }).as('getSessionsAfterCreate');

    cy.intercept('POST', '**/Session', req => {
      expect(req.body.movieId).to.eq(1);
      expect(req.body.cinemaHallId).to.eq(10);
      expect(req.body.createdBy).to.eq(1);
      expect(req.body.price).to.eq(8.5);
      req.reply({ statusCode: 201 });
    }).as('createSession');

    cy.visit('/sessions/create');

    cy.wait('@getMovies');
    cy.wait('@getCinemas');

    cy.get('#session-movie')
      .select('Duna');

    cy.get('#session-cinema')
      .should('not.be.disabled')
      .select('Cinema NOS Colombo');

    cy.wait('@getHallsByCinema');
    cy.wait('@getSeatsHall10');
    cy.wait('@getSeatsHall11');

    cy.get('#session-hall')
      .select('Sala IMAX');

    cy.get('#session-start-date')
      .type('2099-12-31');

    getSessionFormComponent().then(({ cmp, win }) => {

      (win as any).Zone.current.run(() => {
        cmp.selectedTime = '18:00';
        cmp.form.price = 8.5;
        cmp.submit();
      });

    });

    cy.wait('@createSession');
    cy.wait('@getSessionsAfterCreate');

    cy.contains('Duna').should('be.visible');
    cy.contains('Sala IMAX').should('be.visible');

  });

  it('Não deve permitir criar sessão com data passada', () => {

    interceptMovieAndCinemaData();
    interceptHallsAndSeats();
    interceptAvailableTimes(['10:00']);

    const alertStub = cy.stub();
    cy.on('window:alert', alertStub);

    cy.visit('/sessions/create');

    cy.wait('@getMovies');
    cy.wait('@getCinemas');

    cy.get('#session-movie').select('Duna');

    cy.get('#session-cinema')
      .should('not.be.disabled')
      .select('Cinema NOS Colombo');

    cy.wait('@getHallsByCinema');
    cy.wait('@getSeatsHall10');
    cy.wait('@getSeatsHall11');

    cy.get('#session-hall').select('Sala IMAX');

    cy.get('#session-start-date')
      .type('2000-01-01');

    getSessionFormComponent().then(({ cmp, win }) => {

      (win as any).Zone.current.run(() => {
        cmp.selectedTime = '10:00';
        cmp.form.price = 7;
        cmp.submit();
      });

    });

    cy.wrap(null).then(() => {
      expect(alertStub)
        .to.have.been.calledWith('A data de início não pode ser no passado.');
    });

  });

  it('Deve mostrar erro se falhar carregamento de horários', () => {

    interceptMovieAndCinemaData();
    interceptHallsAndSeats();
    interceptAvailableTimes([], 500);

    cy.visit('/sessions/create');

    cy.wait('@getMovies');
    cy.wait('@getCinemas');

    cy.get('#session-movie').select('Duna');

    cy.get('#session-cinema')
      .should('not.be.disabled')
      .select('Cinema NOS Colombo');

    cy.wait('@getHallsByCinema');
    cy.wait('@getSeatsHall10');
    cy.wait('@getSeatsHall11');

    cy.get('#session-hall').select('Sala IMAX');

    cy.get('#session-start-date')
      .type('2099-12-31');

    forceLoadAvailableTimes();

cy.waitUntil(() =>
  getSessionFormComponent().then(({ cmp }) =>
    cmp.errorMessage === 'Erro ao carregar horários.'
  )
);

getSessionFormComponent().then(({ cmp }) => {
  expect(cmp.errorMessage).to.eq('Erro ao carregar horários.');
});
});

  it('Deve carregar sessão e guardar alterações', () => {

    interceptMovieAndCinemaData();
    interceptHallsAndSeats();
    interceptAvailableTimes(['21:00']);

    cy.intercept('GET', '**/Session/1', {
      statusCode: 200,
      body: { ...mockSessions[0], cinemaId: 1 },
    }).as('getSessionById');

    cy.intercept('GET', '**/Session', {
      statusCode: 200,
      body: [{ ...mockSessions[0], price: 11 }],
    }).as('getSessionsAfterEdit');

    cy.intercept('PUT', '**/Session/1', req => {
      expect(req.body.id).to.eq(1);
      expect(req.body.price).to.eq(11);
      req.reply({ statusCode: 200 });
    }).as('updateSession');

    const alertStub = cy.stub();
    cy.on('window:alert', alertStub);

    cy.visit('/sessions/edit/1');

    cy.wait('@getMovies');
    cy.wait('@getCinemas');
    cy.wait('@getSessionById');
    cy.wait('@getHallsByCinema');
    cy.wait('@getAvailableTimes');

    cy.get('#session-movie').invoke('val').should('include', '1');
    cy.get('#session-cinema').invoke('val').should('include', '1');
    cy.get('#session-hall').invoke('val').should('include', '10');

    cy.get('#session-price')
      .clear()
      .type('11');

    cy.get('.btn-create')
      .click({ force: true });

    cy.wait('@updateSession');
    cy.wait('@getSessionsAfterEdit');

    cy.wrap(null).then(() => {

      expect(alertStub)
        .to.have.been.calledWith('Sessão atualizada com sucesso!');

    });

  });

  it('Deve apagar uma sessão da listagem', () => {

    interceptListSessions();

    cy.intercept('DELETE', '**/Session/1', {
      statusCode: 200,
    }).as('deleteSession');

    cy.visit('/sessions');

    cy.wait('@getSessions');

    cy.intercept('GET', '**/Session', {
      statusCode: 200,
      body: mockSessions.filter(s => s.id !== 1),
    }).as('getSessionsAfterDelete');

    cy.on('window:confirm', () => true);

    cy.contains('Duna')
      .closest('tr')
      .find('.btn-small.red')
      .click();

    cy.wait('@deleteSession');
    cy.wait('@getSessionsAfterDelete');

    cy.contains('Duna')
      .should('not.exist');

  });

});
