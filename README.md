# PlayVi

Aplicativo de streaming inspirado na Netflix, feito do zero em **C#**: uma API em ASP.NET Core com SQL Server e um app instalável em .NET MAUI.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)

> Projeto de estudo, em desenvolvimento. O pagamento das assinaturas é **simulado**, e o app foi testado no **Windows**.

<!-- Adicione aqui prints do app. Sugestão: crie a pasta docs/screenshots e use
![Login](docs/screenshots/login.png) -->

## Sobre o projeto

O PlayVi reúne as peças de uma plataforma de streaming real: contas de usuário, planos mensais e anuais, vários perfis por conta, um catálogo de filmes e séries com busca e filtros, e um player de vídeo. O vídeo só é liberado para quem tem uma assinatura ativa, e essa regra é verificada na API, não apenas no app.

## Funcionalidades

**Contas e acesso**
- Cadastro e login com senha protegida por hash (ASP.NET Core Identity)
- Autenticação com token JWT, enviado pelo app em todas as requisições

**Planos e assinatura**
- Planos mensais e anuais
- Assinatura com pagamento simulado, consulta da assinatura atual e cancelamento
- Assinatura vence sozinha: o acesso depende do status *e* da data de término

**Perfis**
- Até 5 perfis por conta, e a conta sempre mantém pelo menos 1
- Escolha de avatar, marcação de perfil infantil, edição e exclusão
- Botão de editar que aparece ao passar o mouse sobre o perfil (no celular fica sempre visível)

**Catálogo e busca**
- Lista de filmes e séries
- Busca por nome e filtros por **gênero**, **época** (antigos e novos) e **tipo** (filme ou série)

**Player**
- Reprodução de vídeo por link, com controles próprios: barra de progresso na largura total, play e pause, pular 10 segundos e controles que somem sozinhos
- Se a pessoa não tem plano, o app oferece a escolha de um e libera o vídeo em seguida

## Arquitetura

```
App instalável (.NET MAUI)  →  API (ASP.NET Core)  →  SQL Server
```

O app **nunca** fala direto com o banco. A API valida quem é o usuário, confere o plano e é a única com acesso aos dados. Assim, a senha do banco não fica dentro do app.

## Tecnologias

| Camada | Tecnologia |
|---|---|
| App | .NET MAUI (XAML + C#), CommunityToolkit.Maui MediaElement |
| API | ASP.NET Core Web API (.NET 10) |
| Banco de dados | SQL Server |
| Acesso a dados | Entity Framework Core (Migrations) |
| Autenticação | ASP.NET Core Identity + JWT |

## Modelo de dados

`Plans` · `Genres` · `Titles` · `TitleGenres` (relação muitos para muitos entre títulos e gêneros) · `Subscriptions` · `Profiles` · tabelas do Identity (`AspNetUsers` e outras)

## Endpoints da API

As rotas protegidas exigem o cabeçalho `Authorization: Bearer <token>`.

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/register` | Cria a conta e devolve um token |
| POST | `/api/auth/login` | Entra e devolve um token |
| GET | `/api/auth/me` | Dados do usuário logado |
| GET | `/api/plans` | Lista os planos |
| POST | `/api/subscriptions` | Assina um plano (pagamento simulado) |
| GET | `/api/subscriptions/me` | Situação da assinatura |
| POST | `/api/subscriptions/cancel` | Cancela a assinatura |
| GET | `/api/genres` | Lista os gêneros |
| GET | `/api/titles` | Catálogo (`q`, `genre`, `era`, `type`) |
| GET | `/api/titles/{id}/watch` | Link do vídeo, **somente com assinatura ativa** (senão, 403) |
| GET / POST | `/api/profiles` | Lista e cria perfis |
| PUT / DELETE | `/api/profiles/{id}` | Edita e apaga um perfil |

## Como rodar localmente

**Pré-requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download), SQL Server Express e o workload do MAUI (`dotnet workload install maui`).

```bash
git clone https://github.com/gramalhoa7/PlayVi.git
cd PlayVi
```

**1. API**

Os segredos não ficam no repositório. Configure-os com User Secrets:

```bash
cd PlayViAPI
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost\SQLEXPRESS;Database=PlayViDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "uma-frase-longa-com-pelo-menos-32-caracteres"

dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run
```

O nome do servidor (`localhost\SQLEXPRESS`) pode ser diferente na sua máquina. A API sobe na porta definida em `Properties/launchSettings.json`.

**2. App (Windows)**

Em outro terminal:

```bash
cd PlayViApp
dotnet run -f net10.0-windows10.0.19041.0
```

O endereço da API está fixo em `PlayViApp/Services/APIService.cs`. Se a sua porta for diferente, ajuste lá.

## Estrutura

```
PlayVi/
├── PlayViAPI/              # API
│   ├── Controllers/        # Auth, Plans, Subscriptions, Titles, Genres, Profiles
│   ├── Data/               # DbContext
│   ├── Models/             # Entidades (viram tabelas)
│   ├── Services/           # TokenService, SubscriptionService
│   └── Migrations/
└── PlayViApp/              # App MAUI
    ├── Services/           # APIService (comunicação com a API), lista de avatares
    ├── MainPage            # Login
    ├── RegisterPage        # Cadastro
    ├── ProfilesPage        # Escolha de perfil
    ├── NewProfilePage      # Novo perfil
    ├── EditProfilePage     # Editar e apagar perfil
    ├── HomePage            # Catálogo, busca e filtros
    └── PlayerPage          # Player de vídeo
```

## Decisões técnicas

- **A regra de negócio fica na API.** O app pode esconder um botão, mas quem barra o acesso ao vídeo é a API, com 403.
- **O link do vídeo nunca aparece na lista do catálogo.** Ele só sai pela rota `watch`, depois da checagem da assinatura.
- **O ID do usuário vem do token**, nunca do corpo da requisição, e toda consulta de perfil e assinatura filtra pelo dono. Assim, ninguém altera dados de outra conta trocando um número na URL.
- **Validação na API.** Nome, limites de perfis, existência do plano e demais entradas são checados no servidor.
- **Assinatura com histórico.** Cancelar muda o status, em vez de apagar o registro.
- **Datas em UTC.**
- **Segredos fora do Git** (User Secrets).

## Próximos passos

- [ ] Séries com temporadas e episódios
- [ ] Tela "Minha conta" (plano atual, cancelamento e sair)
- [ ] Manter o login entre aberturas do app
- [ ] Área de administração para cadastrar títulos
- [ ] Pôsteres no catálogo
- [ ] Classificação indicativa e filtro do perfil infantil
- [ ] Pagamento real (Mercado Pago ou Stripe)
- [ ] API hospedada na internet e testes no Android e iOS

## Aviso sobre conteúdo

Os títulos, links de vídeo e avatares do repositório são **apenas exemplos de desenvolvimento**. Use somente conteúdo seu, em domínio público ou com licença que permita o uso (por exemplo, Creative Commons).

## Autor

Desenvolvido por [@gramalhoa7](https://github.com/gramalhoa7) · [LinkedIn](COLE-SEU-LINK-AQUI)
