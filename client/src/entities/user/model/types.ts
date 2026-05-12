export interface TrainerClient {
  clientId: string;
  email: string;
  fullName: string;
}

export interface TrainerClientProgramAccess {
  assignmentId: string;
  programId: string;
  programTitle: string;
  assignedAtUtc: string;
  expiresAtUtc: string | null;
}

export interface TrainerClientOverview {
  clientId: string;
  email: string;
  fullName: string;
  activePrograms: TrainerClientProgramAccess[];
}
