export interface MovieDto {
  id: number;
  title: string;
  description: string;
  language: string;
  releaseDate?: string;
  country: string;
  posterPath: string;
  genres: string[];
  duration?: number;
}
