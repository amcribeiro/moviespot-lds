export interface VoucherResponseDto {
  id: number;
  code: string;
  value: number;
  validUntil: string;
  maxUsages: number;
  usages: number;
  createdAt: string;
  updatedAt: string;
}
export interface VoucherUpdateDto {
  id: number;
  code: string;
  value: number;
  validUntil: string;
  maxUsages: number;
  usages: number;
}