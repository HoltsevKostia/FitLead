function resolveUploadcarePublicKey(): string {
  const publicKey = process.env.NEXT_PUBLIC_UPLOADCARE_PUBLIC_KEY?.trim();

  if (!publicKey) {
    throw new Error(
      "Environment variable NEXT_PUBLIC_UPLOADCARE_PUBLIC_KEY is required for Uploadcare uploads.",
    );
  }

  return publicKey;
}

export const uploadcareEnv = {
  publicKey: resolveUploadcarePublicKey(),
} as const;
