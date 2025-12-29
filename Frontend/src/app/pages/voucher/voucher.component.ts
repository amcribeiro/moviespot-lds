import { Component, inject } from '@angular/core';
import { CommonModule, DatePipe, PercentPipe }from '@angular/common';
import { VoucherService } from '../../services/voucher.service';
import { VoucherResponseDto } from '../../models/voucher.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
@Component({
  selector: 'app-vouchers',
  standalone: true,
  imports: [CommonModule, DatePipe, PercentPipe, SidebarComponent],
  templateUrl: './voucher.component.html',
  styleUrls: ['./voucher.component.css'],
})
export class VoucherComponent {

  private voucherService = inject(VoucherService);

  vouchers: VoucherResponseDto[] = [];
  loading = false;

  createVoucher() {
    this.loading = true;
    this.voucherService.create().subscribe({
      next: (v) => {
        this.vouchers.unshift(v); 
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
