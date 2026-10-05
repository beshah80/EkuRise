import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Category, CreateCategory, CreateSubCategory, SubCategory,
  SubCategoryDetail, JoinSubCategory, JoinResult, MyEkub,
  SubmitPaymentProof, EkubSubscription
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/catalog';

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/categories`);
  }

  createCategory(data: CreateCategory): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/categories`, data);
  }

  getSubCategories(categoryId: number): Observable<SubCategory[]> {
    return this.http.get<SubCategory[]>(`${this.baseUrl}/categories/${categoryId}/subcategories`);
  }

  getSubCategoryById(id: number): Observable<SubCategoryDetail> {
    return this.http.get<SubCategoryDetail>(`${this.baseUrl}/subcategories/${id}`);
  }

  createSubCategory(data: CreateSubCategory): Observable<SubCategory> {
    return this.http.post<SubCategory>(`${this.baseUrl}/subcategories`, data);
  }

  joinSubCategory(id: number, data: JoinSubCategory): Observable<JoinResult> {
    return this.http.post<JoinResult>(`${this.baseUrl}/subcategories/${id}/join`, data);
  }

  startEkub(id: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/subcategories/${id}/start`, {});
  }

  getMyEkubs(): Observable<MyEkub[]> {
    return this.http.get<MyEkub[]>(`${this.baseUrl}/my-ekubs`);
  }

  submitPaymentProof(subscriptionId: number, data: SubmitPaymentProof): Observable<EkubSubscription> {
    return this.http.post<EkubSubscription>(`${this.baseUrl}/subscriptions/${subscriptionId}/payment-proof`, data);
  }
}
