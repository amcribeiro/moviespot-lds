export interface SessionCreateDto {
  movieId: number;
  cinemaHallId: number;
  createdBy: number;
  startDate: string;
  endDate: string;
  price: number;
}

export interface SessionUpdateDto {
  id: number;
  movieId: number;
  cinemaHallId: number;
  startDate: string;
  endDate: string;
  price: number;
}

export interface SessionResponseDto {
  id: number;
  movieId: number;
  movieTitle: string;
  cinemaHallId: number;
  cinemaHallName: string;
  createdBy: number;
  createdByName: string;
  startDate: string;
  endDate: string;
  price: number;
  createdAt: string;
  updatedAt: string;
}

export interface AvailableSeatDto {
  id: number;
  seatNumber: string;
  seatType: string;
}

export type AvailableTimesResponse = string[];
