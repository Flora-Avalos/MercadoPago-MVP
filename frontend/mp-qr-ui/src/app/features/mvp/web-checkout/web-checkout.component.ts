import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { PaymentService } from '../../../core/services/payment.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { QRCodeComponent } from 'angularx-qrcode';
type PaymentStatus = 'idle' | 'pending' | 'approved' | 'rejected' | 'cancelled';
@Component({
  standalone: true,
  imports: [CommonModule, QRCodeComponent],
  templateUrl: './web-checkout.component.html',
})
export class WebCheckoutComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private signalR = inject(SignalRService);
  private router = inject(Router);

  // 🔵 Signals
  total = signal(0);
  qrCode = signal<string | null>(null);
  externalReference = signal<string | null>(null);
  status = signal<PaymentStatus>('idle');
  loading = signal(false);

  // 🔵 Computed helpers
  showQr = computed(() => !!this.qrCode() && this.status() !== 'approved');

  private normalizeStatus(value: string): PaymentStatus {
    const allowed: PaymentStatus[] = ['idle', 'pending', 'approved', 'rejected', 'cancelled'];

    return allowed.includes(value as PaymentStatus) ? (value as PaymentStatus) : 'idle';
  }

  ngOnInit() {
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    const totalAmount = cart.reduce((a: number, b: any) => a + b.qty * b.price, 0);
    this.total.set(totalAmount);

    this.signalR.startConnection((data) => {
      if (data.externalReference === this.externalReference()) {
        this.status.set(this.normalizeStatus(data.status));

        if (data.status === 'approved') {
          setTimeout(() => {
            this.router.navigate(['/thank-you']);
          }, 1500);
        }
      }
    });
  }

  pay() {
    if (this.total() <= 0) return;

    this.loading.set(true);
    this.status.set('pending');

    this.paymentService.createPayment(this.total(), 'web').subscribe({
      next: (res) => {
        this.externalReference.set(res.externalReference ?? null);
        this.qrCode.set(res.qrCode ?? null);
        this.status.set(this.normalizeStatus(res.status ?? 'pending'));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.status.set('idle');
      },
    });
  }
}
