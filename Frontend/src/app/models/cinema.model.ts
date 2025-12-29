export interface CinemaResponseDto {
  id: number;
  name: string;
  street: string;
  city: string;
  state?: string;
  zipCode?: string;
  country: string;
  latitude: number;
  longitude: number;
  createdAt: string;
  updatedAt: string;
  totalCinemaHalls?: number;
}

export interface CinemaCreateDto {
  name: string;
  street: string;
  city: string;
  state?: string;
  zipCode?: string;
  country: string;
  latitude: number;
  longitude: number;
}

export interface CinemaUpdateDto extends CinemaCreateDto {
  id: number;
}
