import { Component, EventEmitter, Output, AfterViewInit } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-map-picker',
  standalone: true,
  templateUrl: './map-picker.html',
  styleUrls: ['./map-picker.css'],
})
export class MapPickerComponent implements AfterViewInit {
  @Output() locationSelected = new EventEmitter<{
    lat: number;
    lng: number;
    address?: Record<string, string>;
  }>();

  map!: L.Map;
  marker?: L.Marker;
  
  private markerIcon = L.icon({
    iconUrl: 'assets/leaflet/marker-icon.png',
    iconRetinaUrl: 'assets/leaflet/marker-icon-2x.png',
    shadowUrl: 'assets/leaflet/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    shadowSize: [41, 41],
  });

  ngAfterViewInit() {
    this.map = L.map('map', {
      center: [38.7223, -9.1393],
      zoom: 13,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 18,
    }).addTo(this.map);
    setTimeout(() => {
      this.map.invalidateSize();
    },100);

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      const lat = e.latlng.lat;
      const lng = e.latlng.lng;

      if (this.marker) {
        this.marker.setLatLng([lat, lng]);
      } else {
        this.marker = L.marker([lat, lng], {
          draggable: true,
          icon: this.markerIcon,
        }).addTo(this.map);

        this.marker.on('dragend', () => {
          const pos = this.marker!.getLatLng();
          this.reverseGeocode(pos.lat, pos.lng);
        });
      }

      this.reverseGeocode(lat, lng);
    });
  }

  async reverseGeocode(lat: number, lng: number) {
    const url =
      `https://nominatim.openstreetmap.org/reverse?lat=${lat}` +
      `&lon=${lng}&format=json&addressdetails=1`;

    const response = await fetch(url);
    const data = (await response.json()) as { address?: Record<string, string> };

    this.locationSelected.emit({
      lat,
      lng,
      address: data.address,
    });
  }
}
