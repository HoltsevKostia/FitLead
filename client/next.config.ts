import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  images: {
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
    ],
  },
  reactCompiler: true,
};

export default nextConfig;
