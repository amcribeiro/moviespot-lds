/// <reference types="cypress" />

declare namespace Cypress {
  interface Chainable {
    loginMock(): Chainable<void>;

    simulateMapSelection(
      lat: number,
      lng: number,
      address: {
        road?: string;
        street?: string;
        city?: string;
        state?: string;
        county?: string;
        region?: string;
        postcode?: string;
        zipCode?: string;
        country?: string;
        [key: string]: any;
      }
    ): Chainable<void>;
  }
}

Cypress.Commands.add('loginMock', () => {

  cy.window().then(win => {
    win.localStorage.setItem('token', 'fake-jwt-cypress-admin');
    win.localStorage.setItem('refreshToken', 'fake-refresh-token-cypress');
  });

  cy.log('Auth Tokens Injetados 💉');
});

Cypress.Commands.add(
  'simulateMapSelection',
  (lat: number, lng: number, address: any) => {

    cy.log(`Simulação mapa real: ${lat}, ${lng}`);

    cy.get('app-cinema-form')
      .should('exist')
      .then($el => {

        cy.window().then(win => {

          const ng = (win as any).ng;
          if (!ng?.getComponent) {
            throw new Error('Angular DevTools indisponível');
          }

          const component = ng.getComponent($el[0]);
          if (!component) {
            throw new Error('app-cinema-form não encontrado');
          }

          const mappedAddress = {
            road: address.road ?? address.street ?? '',
            city: address.city ?? '',
            state: address.state ?? address.county ?? address.region ?? '',
            postcode: address.postcode ?? address.zipCode ?? '',
            country: address.country ?? ''
          };

          if (!mappedAddress.state) {
            throw new Error('STATE é obrigatório no simulateMapSelection');
          }

          (win as any).Zone.current.run(() => {

            component.updateLocation({
              lat,
              lng,
              address: mappedAddress
            });

          });

        });

      });

  }
);
