export interface TransferResult {
  transactionId: string;
  referenceNumber: string;
  amount: number;
  fee: number;
  status: string;
  completedAtUtc: string;
}

export interface BeneficiaryCandidate {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  alreadySaved: boolean;
}

export interface Beneficiary {
  id: string;
  beneficiaryUserId: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string | null;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
