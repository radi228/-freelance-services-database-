# 🚀 Skillo – Пазар за фрийланс услуги

ASP.NET Core 8 Web API + SQLite + JWT + Stripe + PayPal

## ▶️ Стартиране

```bash
cd SkilloPlatform
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Сайт:    http://localhost:5000  
Swagger: http://localhost:5000/swagger

## 🧪 Тестове

```bash
cd SkilloPlatform.Tests
dotnet test --verbosity normal
```

## 👤 Демо акаунти

| Роля        | Имейл                    | Парола     |
|-------------|--------------------------|------------|
| SuperAdmin  | superadmin@skillo.bg     | Demo1234!  |
| Admin       | admin@skillo.bg          | Demo1234!  |
| Freelancer  | alex@skillo.bg           | Demo1234!  |
| Client      | client@techstart.bg      | Demo1234!  |

## 📋 Entities (7)

- **User** – потребители с роли
- **FreelancerProfile** – профил на фрийлансър
- **Project** – проекти на клиенти
- **Bid** – оферти от фрийлансъри
- **Service** – готови услуги
- **Payment** – плащания (Stripe / PayPal / Simulated)
- **Review** – отзиви и рейтинги

## 🎮 Controllers (7)

- **AuthController** – register, login
- **FreelancersController** – профили, avatar
- **ProjectsController** – CRUD проекти
- **BidsController** – CRUD оферти
- **ServicesController** – CRUD услуги
- **PaymentsController** – Stripe, PayPal, Simulated, Refund
- **AdminController** – пълен админ панел

## 💳 Плащания

- **Stripe** – `POST /api/payments/stripe`
- **PayPal** – `POST /api/payments/paypal`
- **Симулирани** – `POST /api/payments/simulated`
- **Refund** – `POST /api/payments/{id}/refund`

## 🌐 Deploy на Render

1. Качи на GitHub
2. Отиди на render.com → New Web Service
3. Свържи репото
4. Render автоматично чете `render.yaml`
5. Добави Stripe и PayPal ключове в Environment Variables
