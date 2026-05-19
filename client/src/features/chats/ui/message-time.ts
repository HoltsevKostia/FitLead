const messageTimeFormatter = new Intl.DateTimeFormat("uk-UA", {
  hour: "2-digit",
  minute: "2-digit",
});

export function formatMessageTime(value: string): string {
  return messageTimeFormatter.format(new Date(value));
}
