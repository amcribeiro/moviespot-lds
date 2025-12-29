export interface SeatCreateDto {
  cinemaHallId: number;
  seatNumber: string;
  seatType: string;
}

export interface SeatUpdateDto {
  id: number;
  cinemaHallId: number;
  seatNumber: string;
  seatType: string;
}

export interface SeatResponseDto {
  id: number;
  cinemaHallId: number;
  seatNumber: string;
  seatType: string;
  createdAt: string;
  updatedAt: string;
}
