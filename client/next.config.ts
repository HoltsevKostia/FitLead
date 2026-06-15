import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: process.env.FITLEAD_SMOKE_TEST === "true"
    ? undefined
    : "standalone",
  images: {
    unoptimized: process.env.FITLEAD_SMOKE_TEST === "true",
    remotePatterns: [
      {
        protocol: "https",
        hostname: "ucarecdn.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "*.ucarecd.net",
        pathname: "/**",
      },
      {
        protocol: "http",
        hostname: "localhost",
        port: "3000",
        pathname: "/smoke/**",
      },
    ],
  },
  reactCompiler: true,
};

export default nextConfig;
