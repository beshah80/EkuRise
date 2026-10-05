// ===== Auth Models =====

export interface RegisterRequest {
  phoneNumber: string;
  firstName: string;
  lastName: string;
  gender: number;
  jobType: string;
  location: string;
  referralCode?: string;
}

export interface VerifyRegistration {
  phoneNumber: string;
  code: string;
}

export interface SendOtp {
  phoneNumber: string;
}

export interface VerifyOtp {
  phoneNumber: string;
  code: string;
}

export interface PinLogin {
  phoneNumber: string;
  pin: string;
}

export interface SetPin {
  pin: string;
  currentCredential: string;
}

export interface UpdateProfile {
  email?: string;
  firstName?: string;
  lastName?: string;
  gender?: number;
  jobType?: string;
  location?: string;
  profilePictureUrl?: string;
  preferredLanguage?: number;
}

export interface AuthResponse {
  token: string;
  userId: number;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  isPhoneVerified: boolean;
  isAdmin: boolean;
}

export interface OtpSent {
  phoneNumber: string;
  message: string;
  demoCode?: string;
}

export interface UserProfile {
  id: number;
  phoneNumber: string;
  email?: string;
  firstName: string;
  lastName: string;
  gender: number;
  jobType: string;
  location: string;
  profilePictureUrl?: string;
  isPhoneVerified: boolean;
  isPinEnabled: boolean;
  preferredLanguage: number;
  referralCode: string;
  referredByCode?: string;
  isAdmin: boolean;
  createdAt: string;
}

export interface UserDto {
  id: number;
  fullName: string;
  phoneNumber: string;
  profilePictureUrl?: string;
}

// ===== Catalog Models =====

export interface Category {
  id: number;
  name: string;
  description?: string;
  iconUrl?: string;
  subCategoryCount: number;
}

export interface CreateCategory {
  name: string;
  description?: string;
  iconUrl?: string;
}

export interface CreateSubCategory {
  categoryId: number;
  name: string;
  dailyContribution: number;
  totalRounds: number;
  startDate: string;
  termsAndConditions: string;
  maxMembers: number;
}

export interface SubCategory {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  dailyContribution: number;
  totalRounds: number;
  totalAmount: number;
  startDate: string;
  maxMembers: number;
  currentMemberCount: number;
  status: number;
  hasJoined: boolean;
  circleId?: number;
}

export interface SubCategoryDetail {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  dailyContribution: number;
  totalRounds: number;
  totalAmount: number;
  startDate: string;
  termsAndConditions: string;
  maxMembers: number;
  currentMemberCount: number;
  status: number;
  hasJoined: boolean;
  circleId?: number;
  subscriptionId?: number;
  subscriptionStatus?: number;
  submittedFullName?: string;
  submittedNationalIdFan?: string;
  submittedPaymentProofUrl?: string;
  rejectionReason?: string;
}

export interface JoinSubCategory {
  agreedToTerms: boolean;
}

export interface JoinResult {
  subscriptionId: number;
  subCategoryId: number;
  subCategoryName: string;
  dailyContribution: number;
  totalAmount: number;
  startDate: string;
  message: string;
  subscriptionStatus?: number;
}

export interface SubmitPaymentProof {
  fullName: string;
  nationalIdFan: string;
  paymentProofUrl: string;
}

export interface EkubSubscription {
  id: number;
  userId: number;
  userPhoneNumber: string;
  subCategoryId: number;
  subCategoryName: string;
  categoryName: string;
  dailyContribution: number;
  totalAmount: number;
  fullName?: string;
  nationalIdFan?: string;
  paymentProofUrl?: string;
  status: number;
  rejectionReason?: string;
  joinedAt: string;
  submittedAt?: string;
  approvedAt?: string;
}

export interface MyEkub {
  subscriptionId: number;
  subCategoryId: number;
  categoryName: string;
  subCategoryName: string;
  dailyContribution: number;
  totalAmount: number;
  startDate: string;
  status: number;
  joinedAt: string;
  circleId?: number;
  subscriptionStatus?: number;
}

// ===== Circle Models =====

export interface CreateCircle {
  name: string;
  contribution: number;
  meetingLabel: string;
}

export interface AddMember {
  phoneNumber: string;
}

export interface MemberDto {
  userId: number;
  fullName: string;
  phoneNumber: string;
  profilePictureUrl?: string;
  payoutOrder: number;
  hasReceived: boolean;
  joinedAt: string;
}

export interface CircleSummary {
  id: number;
  name: string;
  contribution: number;
  meetingLabel: string;
  status: number;
  memberCount: number;
  currentRoundNumber: number;
  organizerName: string;
}

export interface CircleDetail {
  id: number;
  name: string;
  contribution: number;
  meetingLabel: string;
  status: number;
  organizerName: string;
  currentRoundNumber?: number;
  memberCount: number;
  members: MemberDto[];
}

// ===== Round Models =====

export interface MarkPayment {
  userId: number;
  hasPaid: boolean;
  lateFine?: number;
}

export interface PaymentDto {
  userId: number;
  memberName: string;
  profilePictureUrl?: string;
  hasPaid: boolean;
  paidAt?: string;
  lateFine: number;
}

export interface RoundDetail {
  id: number;
  roundNumber: number;
  totalRounds: number;
  status: number;
  contribution: number;
  pot: number;
  paidCount: number;
  totalMembers: number;
  receiverId?: number;
  receiverName?: string;
  openedAt: string;
  paidOutAt?: string;
  payments: PaymentDto[];
}

export interface RoundSummary {
  id: number;
  roundNumber: number;
  status: number;
  pot: number;
  paidCount: number;
  totalMembers: number;
  receiverId?: number;
  receiverName?: string;
  openedAt: string;
  paidOutAt?: string;
}

export interface PayoutResult {
  roundId: number;
  roundNumber: number;
  receiverId: number;
  receiverName: string;
  potAmount: number;
  paidOutAt: string;
}

// ===== Notification Models =====

export interface NotificationDto {
  id: number;
  type: number;
  title: string;
  body: string;
  isRead: boolean;
  relatedCircleId?: number;
  relatedRoundId?: number;
  createdAt: string;
}

// ===== Feedback Models =====

export interface CreateFeedback {
  subject: string;
  message: string;
}

export interface FeedbackDto {
  id: number;
  subject: string;
  message: string;
  status: number;
  createdAt: string;
}

export interface CreateSuccessStory {
  content: string;
  rating: number;
  authorName?: string;
}

export interface DeleteAccountRequest {
  phoneNumber: string;
}

export interface ConfirmDeleteAccount {
  code: string;
}

export interface SuccessStoryDto {
  id: number;
  authorName: string;
  content: string;
  rating: number;
  createdAt: string;
}

// ===== Admin Portal Models =====

export interface AdminStats {
  totalUsers: number;
  totalCircles: number;
  activeCircles: number;
  formingCircles: number;
  completedCircles: number;
  totalCategories: number;
  totalSubCategories: number;
  pendingStories: number;
  pendingFeedbacks: number;
  totalSavingsVolume: number;
  pendingSubscriptions: number;
}

export interface AdminUser {
  id: number;
  phoneNumber: string;
  email?: string;
  firstName: string;
  lastName: string;
  jobType: string;
  location: string;
  isPhoneVerified: boolean;
  isAdmin: boolean;
  joinedCirclesCount: number;
  createdAt: string;
}

export interface AdminStory {
  id: number;
  userId?: number;
  authorName: string;
  userPhoneNumber?: string;
  content: string;
  rating: number;
  isApproved: boolean;
  createdAt: string;
}

export interface AdminFeedback {
  id: number;
  userId: number;
  userName: string;
  userPhoneNumber: string;
  subject: string;
  message: string;
  status: number;
  createdAt: string;
}

