import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="min-h-screen flex flex-col items-center justify-center gap-6">
    <h1 class="text-3xl font-bold text-green-600">
      Gracias por su compra
    </h1>
    <button class="bg-blue-600 text-white px-6 py-3 rounded-xl"
            (click)="goHome()">
      Inicio
    </button>
  </div>
  `
})
export class ThankYouComponent {
  constructor(private router: Router) {}

  goHome() {
    localStorage.clear();
    this.router.navigate(['/']);
  }
}
