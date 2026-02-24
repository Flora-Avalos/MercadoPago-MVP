// import { Injectable, inject } from '@angular/core';
// import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

// @Injectable({
//   providedIn: 'root',
// })
// export class PaymentService {
//   private http = inject(HttpClient);
//   private baseUrl = `${environment.apiUrl}/api/payments`;

//   createPayment(amount: number) {
//     return this.http.post<any>(this.baseUrl, { amount });
//   }

//   getStatus(externalReference: string) {
//     return this.http.get<{ status: string }>(`${this.baseUrl}/${externalReference}/status`);
//   }
//   cancelPayment(externalReference: string) {
//     return this.http.post<{ status: string }>(`${this.baseUrl}/${externalReference}/cancel`, {});
//   }
// }
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreatePaymentResponse {
  externalReference: string;
  qrCode: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {

  private baseUrl = `${environment.apiUrl}/api/payments`;
  // private apiUrl = 'https://localhost:5001/api/payments';

  constructor(private http: HttpClient) {}

  createPayment(amount: number, mode: 'web' | 'store'): Observable<CreatePaymentResponse> {
    return this.http.post<CreatePaymentResponse>(this.baseUrl, {
      amount,
      mode
    });
  }
}
