import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { NgForm } from '@angular/forms';
import { Setup } from '../models/setup';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {

  configEndpoint: string = window.location.origin + "/api/config";

  constructor(private http: HttpClient) { }

  getConfig() {
    return this.http.get(this.configEndpoint);
  }

  saveConfig(setup: Setup) {
    return this.http.post(this.configEndpoint, setup);
  }

}
