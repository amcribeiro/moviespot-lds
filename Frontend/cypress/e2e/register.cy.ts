describe('Página de Registo (RegisterComponent)', () => {
  
  beforeEach(() => {
    // Visita a página de registo antes de cada teste
    cy.visit('/register');
  });

  it('deve carregar os elementos da página corretamente', () => {
    cy.contains('h2', 'Criar Conta Staff').should('be.visible');
    cy.get('input[name="name"]').should('be.visible');
    cy.get('input[name="email"]').should('be.visible');
    cy.get('input[name="password"]').should('be.visible');
    cy.get('input[name="phone"]').should('be.visible');
    cy.get('button[type="submit"]').should('contain', 'Criar Conta Staff');
  });

  it('deve mostrar erro de validação se tentar submeter vazio', () => {
    // Tenta submeter sem preencher nada
    cy.get('button[type="submit"]').click();

    // Verifica a mensagem de erro definida no component.ts
    cy.get('.error-message')
      .should('be.visible')
      .and('contain', 'Preenche todos os campos obrigatórios.');
  });

  it('deve registar um utilizador com sucesso e redirecionar para o login', () => {
    // Mock da resposta do backend (Status 200 OK)
    cy.intercept('POST', '**/User/register', {
      statusCode: 200,
      body: { 
        accessToken: 'fake-jwt-token', 
        refreshToken: 'fake-refresh-token' 
      }
    }).as('registerRequest');

    // Preencher o formulário
    cy.get('input[name="name"]').type('João Teste');
    cy.get('input[name="email"]').type('joao@teste.com');
    cy.get('input[name="password"]').type('123456');
    cy.get('input[name="phone"]').type('912345678');

    // Submeter
    cy.get('button[type="submit"]').click();

    // Verificar estado de loading (opcional, pois é muito rápido no mock)
    cy.get('button[type="submit"]').should('be.disabled').and('contain', 'A criar conta...');

    // Esperar pela chamada à API
    cy.wait('@registerRequest').then((interception) => {
      // Validar o payload enviado
      expect(interception.request.body).to.deep.equal({
        name: 'João Teste',
        email: 'joao@teste.com',
        password: '123456',
        phone: '912345678',
        role: 'Staff'
      });
    });

    // Validar redirecionamento para o login
    cy.url().should('include', '/login');
  });

  it('deve exibir mensagem de erro quando a API falha (ex: email duplicado)', () => {
    // Mock de erro do backend (Status 400 ou 409)
    const errorMessage = 'Este email já está em uso.';
    
    cy.intercept('POST', '**/User/register', {
      statusCode: 400,
      body: { message: errorMessage }
    }).as('registerError');

    // Preencher formulário
    cy.get('input[name="name"]').type('Maria Erro');
    cy.get('input[name="email"]').type('existente@teste.com');
    cy.get('input[name="password"]').type('123456');

    cy.get('button[type="submit"]').click();

    cy.wait('@registerError');

    // Verificar se a mensagem de erro vinda da API é exibida
    cy.get('.error-message')
      .should('be.visible')
      .and('contain', errorMessage);
    
    // Verificar que NÃO redirecionou
    cy.url().should('include', '/register');
  });

  it('deve navegar para o login ao clicar no link "Já tens conta?"', () => {
    // Clica no link de voltar ao login
    cy.contains('a', 'Já tens conta? Inicia sessão').click();

    // Verifica a URL
    cy.url().should('include', '/login');
  });

});