export interface PaymentIntentDto {
  reference: string;
  amountInCents: number;
  currency: string;
  publicKey: string;
  signature: string;
  redirectUrl: string;
}
 