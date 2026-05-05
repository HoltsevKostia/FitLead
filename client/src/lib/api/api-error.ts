export interface ApiProblemDetails {
  title: string | null;
  detail: string | null;
  errorCode: string | null;
  errors: Record<string, string[]> | null;
}

interface ApiErrorOptions extends ApiProblemDetails {
  status: number;
}

function isValidationErrors(value: unknown): value is Record<string, string[]> {
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    return false;
  }

  return Object.values(value).every(
    (entry) => Array.isArray(entry) && entry.every((item) => typeof item === "string"),
  );
}

function isJsonContentType(contentType: string | null): boolean {
  return (
    contentType?.includes("application/json") === true ||
    contentType?.includes("+json") === true
  );
}

async function readProblemDetails(response: Response): Promise<ApiProblemDetails> {
  const contentType = response.headers.get("content-type");

  if (!isJsonContentType(contentType)) {
    return {
      title: response.statusText || null,
      detail: null,
      errorCode: null,
      errors: null,
    };
  }

  try {
    const payload = (await response.json()) as Record<string, unknown>;

    return {
      title: typeof payload.title === "string" ? payload.title : response.statusText || null,
      detail: typeof payload.detail === "string" ? payload.detail : null,
      errorCode: typeof payload.errorCode === "string" ? payload.errorCode : null,
      errors: isValidationErrors(payload.errors) ? payload.errors : null,
    };
  } catch {
    return {
      title: response.statusText || null,
      detail: null,
      errorCode: null,
      errors: null,
    };
  }
}

export class ApiError extends Error {
  readonly status: number;
  readonly title: string | null;
  readonly detail: string | null;
  readonly errorCode: string | null;
  readonly errors: Record<string, string[]> | null;

  constructor(options: ApiErrorOptions) {
    super(options.detail ?? options.title ?? "API request failed.");
    this.name = "ApiError";
    this.status = options.status;
    this.title = options.title;
    this.detail = options.detail;
    this.errorCode = options.errorCode;
    this.errors = options.errors;
  }

  static async fromResponse(response: Response): Promise<ApiError> {
    const problem = await readProblemDetails(response);

    return new ApiError({
      status: response.status,
      title: problem.title,
      detail: problem.detail,
      errorCode: problem.errorCode,
      errors: problem.errors,
    });
  }
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}
