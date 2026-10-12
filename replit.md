# Running this project on Replit

The repository contains an Angular frontend in `TaskAi-Frontend/` and an ASP.NET Core 10 API in `Task List Application with AI Integration/Task List Application with AI Integration/`. Run the configured **Start application** workflow to start both.

The API uses the Replit PostgreSQL `DATABASE_URL` when `ConnectionStrings__DefaultConnection` is not set. Apply the checked-in Entity Framework migrations from the API project directory with `dotnet ef database update`. The development database starts empty.

JWT signing uses the existing `SESSION_SECRET`; it must be at least 32 bytes. Configure `GEMINI_API_KEY` in Replit Secrets to enable Gemini AI features. AI features require a Google AI Studio API key; other task and authentication API endpoints do not.

For local development, start the API on port 5001 and the Angular server on port 5000. Angular proxies `/api` requests to the API. Install frontend packages from `TaskAi-Frontend/` using `npm install`.
