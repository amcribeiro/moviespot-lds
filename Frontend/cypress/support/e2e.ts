beforeEach(() => {
  cy.clearLocalStorage();

  cy.intercept('POST', '**/User/refresh*', {
    statusCode: 200,
    body: {
      accessToken: 'cypress-refresh-access',
      refreshToken: 'cypress-refresh-refresh',
    },
  }).as('refreshReq');
});

import './commands';
