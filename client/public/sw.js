self.addEventListener("push", (event) => {
  let payload = {};

  if (event.data) {
    try {
      payload = event.data.json();
    } catch {
      payload = {};
    }
  }

  const title = payload.title || "Нове сповіщення у FitLead";
  const options = {
    body: payload.body || "Відкрийте FitLead, щоб переглянути деталі.",
    icon: "/window.svg",
    badge: "/window.svg",
    data: {
      url: payload.url || "/dashboard",
    },
  };

  event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();

  const targetUrl = "/dashboard";

  event.waitUntil(
    self.clients
      .matchAll({ type: "window", includeUncontrolled: true })
      .then((clients) => {
        const target = new URL(targetUrl, self.location.origin);

        for (const client of clients) {
          const url = new URL(client.url);
          if (url.origin === target.origin && "focus" in client) {
            if ("navigate" in client) {
              return client.navigate(target.href).then((navigatedClient) =>
                navigatedClient ? navigatedClient.focus() : client.focus(),
              );
            }

            return client.focus();
          }
        }

        if (self.clients.openWindow) {
          return self.clients.openWindow(target.href);
        }

        return undefined;
      }),
  );
});
