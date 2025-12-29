import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  VoucherResponseDto,
  VoucherUpdateDto,
} from '../models/voucher.model';

@Injectable({
  providedIn: 'root',
})
export class VoucherService {
  private baseUrl = 'http://localhost:5000/Voucher';
  private http = inject(HttpClient);

  create(): Observable<VoucherResponseDto> {
    return this.http.post<VoucherResponseDto>(`${this.baseUrl}`, {});
  }

  getById(id: number): Observable<VoucherResponseDto> {
    return this.http.get<VoucherResponseDto>(`${this.baseUrl}/${id}`);
  }

  update(id: number, data: VoucherUpdateDto): Observable<VoucherResponseDto> {
    return this.http.put<VoucherResponseDto>(`${this.baseUrl}/${id}`, data);
  }

}