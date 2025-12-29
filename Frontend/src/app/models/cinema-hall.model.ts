export interface CinemaHallReadDto {
  id: number;
  name: string;
  cinemaId: number;
}

export interface CinemaHallCreateDto {
  name: string;
  cinemaId: number;
}

export interface CinemaHallUpdateDto {
  id: number;
  name: string;
  cinemaId: number;
}

export interface CinemaHallDetailsDto {
  id: number;
  name: string;
  cinemaId: number;
  cinemaName?: string;
  createdAt: string;   
  updatedAt: string;   
}
