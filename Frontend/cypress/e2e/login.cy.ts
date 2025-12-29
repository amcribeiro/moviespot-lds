const validJwt =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9' +
  '.eyJpZCI6MSwiZXhwIjoxOTk5OTk5OTk5fQ' +
  '.dummy-signature';

describe('Auth / Login flow', () => {

  beforeEach(() => {
    cy.clearLocalStorage();
  });

  it('desativa botão se faltar email/password', () => {
    cy.visit('/login');

    cy.get('button[type="submit"]')
      .should('exist')
      .and('be.disabled');
  });

  it('login com sucesso guarda tokens e vai para dashboard', () => {

    cy.intercept('POST', '**/User/login*', {
      statusCode: 200,
      body: {
        accessToken: validJwt,
        refreshToken: 'fake.jwt.refresh',
      },
    }).as('loginReq');

    cy.intercept('GET', '**/User/1', {
      statusCode: 200,
      body: { name: 'Miguel' },
    }).as('profileReq');

    cy.intercept('GET', '**/Stats*', {
      statusCode: 200,
      body: {
        totalSessions: 10,
        activeRooms: 3,
        todaysSessions: 2,
        movies: 5,
      },
    }).as('statsReq');

    cy.visit('/login');

    cy.get('#email').type('test@mail.com');
    cy.get('#password').type('123456');

    cy.get('button[type="submit"]').click();

    cy.wait('@loginReq');
    cy.wait('@profileReq');
    cy.wait('@statsReq');

    cy.window().then((win) => {
      expect(win.localStorage.getItem('token')).to.eq(validJwt);
      expect(win.localStorage.getItem('refreshToken')).to.eq('fake.jwt.refresh');
    });

    cy.location('pathname', { timeout: 10000 })
      .should('eq', '/dashboard');
  });

  it('login inválido não grava token e mantém /login', () => {

    cy.intercept('POST', '**/User/login*', {
      statusCode: 401,
      body: { message: 'Credenciais inválidas' },
    }).as('loginReq');

    cy.visit('/login');

    cy.get('#email').type('bad@mail.com');
    cy.get('#password').type('wrong');

    cy.get('button[type="submit"]').click();

    cy.wait('@loginReq');

    cy.location('pathname', { timeout: 10000 })
      .should('eq', '/login');

    cy.location('pathname')
      .should('not.eq', '/dashboard');

    cy.window().then((win) => {
      expect(win.localStorage.getItem('token')).to.eq(null);
      expect(win.localStorage.getItem('refreshToken')).to.eq(null);
    });


    cy.get('button[type="submit"]', { timeout: 10000 })
      .should('contain.text', 'Entrar')
  });

  it('AuthGuard bloqueia /dashboard sem token', () => {

    cy.clearLocalStorage();

    cy.visit('/dashboard');

    cy.location('pathname', { timeout: 10000 })
      .should('eq', '/login');
  });

  it('logout limpa tokens e redireciona para /login', () => {

    cy.window().then((win) => {
      win.localStorage.setItem('token', validJwt);
      win.localStorage.setItem('refreshToken', 'fake.jwt.refresh');
    });

    cy.intercept('GET', '**/User/1', {
      statusCode: 200,
      body: { name: 'Miguel' },
    }).as('profileReq');

    cy.intercept('GET', '**/Stats*', {
      statusCode: 200,
      body: {
        totalSessions: 10,
        activeRooms: 3,
        todaysSessions: 2,
        movies: 5,
      },
    }).as('statsReq');

    cy.visit('/dashboard');

    cy.wait('@profileReq');
    cy.wait('@statsReq');

    cy.get('button.sign-out', { timeout: 10000 })
      .should('be.visible')
      .click();

    cy.window().then((win) => {
      expect(win.localStorage.getItem('token')).to.eq(null);
      expect(win.localStorage.getItem('refreshToken')).to.eq(null);
    });

    cy.location('pathname', { timeout: 10000 })
      .should('eq', '/login');
  });

});
