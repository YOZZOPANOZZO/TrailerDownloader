import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Setup } from '../models/setup';
import { Config } from '../models/config';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {

  configEndpoint: string = window.location.origin + "/api/config";

  constructor(private http: HttpClient) { }

  getConfig() {
    return this.http.get<Config>(this.configEndpoint);
  }

  saveConfig(setup: Setup) {
    return this.http.post(this.configEndpoint, setup);
  }

}
