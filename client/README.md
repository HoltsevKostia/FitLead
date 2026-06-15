This is the FitLead Next.js client.

## Local smoke tests

The Playwright smoke suite uses a real production Next.js server, ASP.NET API,
an isolated PostgreSQL container, and the existing demo seed. The orchestrator
does not modify the normal development database or its User Secrets connection
string.

Start Docker Desktop. Then, from `FitLead/client`, run:

```powershell
npm ci
npm run test:smoke:local
```

The script:

1. creates a disposable `postgres:16-alpine` container on a random host port;
2. applies EF Core migrations;
3. starts the API with temporary smoke-test configuration and demo seed;
4. installs Chromium if needed;
5. builds and starts the production frontend;
6. runs the Playwright suite;
7. stops the API and removes the PostgreSQL container.

Ports `3000` and `5178` must be free. The script cleans up temporary resources
even when migrations, startup, or a smoke test fails.

The seeded trainer credentials are:

```text
demo.trainer@fitlead.local
Demo123!
```

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
