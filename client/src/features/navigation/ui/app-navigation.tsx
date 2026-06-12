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
    links: [
      { href: "/chats", label: "Чати" },
      { href: "/video-reports", label: "Відеозвіти" },
    ],
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
      prefetch={false}
      aria-current={isActive ? "page" : undefined}
      className={`inline-flex min-h-10 items-center rounded-lg px-3 py-2 text-sm font-medium transition max-[529px]:flex max-[529px]:w-full lg:flex lg:w-full ${
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
      className="flex flex-wrap items-start gap-2 max-[529px]:grid max-[529px]:grid-cols-1 max-[529px]:gap-3 lg:grid lg:grid-cols-1 lg:gap-3"
    >
      {groups.map((group) => (
        <div
          key={group.links[0].href}
          className="flex min-w-0 flex-wrap gap-1 rounded-xl border border-border bg-surface-strong/35 p-1 max-[529px]:block max-[529px]:w-full lg:block"
        >
          {group.links.map((link) => (
            <NavigationItem key={link.href} link={link} pathname={pathname} />
          ))}
        </div>
      ))}
    </nav>
  );
}
