"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

interface NavigationLink {
  href: string;
  label: string;
}

interface NavigationGroup {
  links: NavigationLink[];
}

const trainerNavigationGroups: NavigationGroup[] = [
  {
    links: [{ href: "/dashboard", label: "Панель" }],
  },
  {
    links: [
      { href: "/clients", label: "Клієнти" },
      { href: "/invitations", label: "Запрошення" },
    ],
  },
  {
    links: [{ href: "/chats", label: "Чати" }],
  },
  {
    links: [
      { href: "/exercises", label: "Вправи" },
      { href: "/workouts", label: "Тренування" },
      { href: "/training-programs", label: "Програми" },
    ],
  },
];

const clientNavigationGroups: NavigationGroup[] = [
  {
    links: [
      { href: "/dashboard", label: "Панель" },
      { href: "/chats", label: "Чати" },
      { href: "/client/training-programs", label: "Мої програми" },
      { href: "/client/profile", label: "Профіль" },
    ],
  },
];

function isActivePath(pathname: string, href: string): boolean {
  return pathname === href || pathname.startsWith(`${href}/`);
}

function NavigationItem({
  link,
  pathname,
}: {
  link: NavigationLink;
  pathname: string;
}) {
  const isActive = isActivePath(pathname, link.href);

  return (
    <Link
      href={link.href}
      aria-current={isActive ? "page" : undefined}
      className={`block rounded-lg px-3 py-2.5 text-sm font-medium transition ${
        isActive
          ? "bg-accent text-white shadow-sm"
          : "text-foreground hover:bg-white"
      }`}
    >
      {link.label}
    </Link>
  );
}

export function AppNavigation({ role }: { role: "Trainer" | "Client" }) {
  const pathname = usePathname();
  const groups = role === "Trainer" ? trainerNavigationGroups : clientNavigationGroups;

  return (
    <nav
      aria-label="Основна навігація"
      className="grid gap-3 sm:grid-cols-2 lg:grid-cols-1"
    >
      {groups.map((group, index) => (
        <div
          key={group.links[0].href}
          className={`min-w-0 rounded-xl border border-border bg-surface-strong/35 p-1 ${
            role === "Client" || index === groups.length - 1 ? "sm:col-span-2 lg:col-span-1" : ""
          }`}
        >
          {group.links.map((link) => (
            <NavigationItem key={link.href} link={link} pathname={pathname} />
          ))}
        </div>
      ))}
    </nav>
  );
}
