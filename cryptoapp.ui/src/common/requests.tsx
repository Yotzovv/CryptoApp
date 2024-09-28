import axios from "axios";

interface AuthResponse {
  token: string;
}

interface RegisterResponse {
  message: string;
}

export interface AspNetUserDto {
  id: string;
  email: string | null;
}

export interface CurrencyDto {
  id: string;
  coin: string;
  amount: number;
  initialBuyPrice: number;
  currentPrice: number;
  changePercentage: number;
}

export interface PortfolioDto {
  id: string;
  initialPortfolioValue: number;
  currentPortfolioValue: number;
  overallChangePercentage: number;
  currencies: CurrencyDto[];
  user: AspNetUserDto;
}


const api = axios.create({
  baseURL: "http://localhost:5088/api/",
  withCredentials: true,
});

api.interceptors.request.use(
  (config) => {
    const token = sessionStorage.getItem("jwt");
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    } else {
      delete config.headers["Authorization"];
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      sessionStorage.removeItem("jwt");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

const login = (email: string, password: string) =>
  api
    .post<AuthResponse>("Auth/login", { Email: email, Password: password },
      { headers: { "Content-Type": "application/json" } } )
    .then((res) => {
      const token = res.data.token;
      if (token) {
        sessionStorage.setItem("jwt", token);
        window.location.href = "/home";
      } else {
        throw new Error("Token not found in response");
      }
    })
    .catch((error) => {
      console.error("Login failed:", error);
      alert("Login failed. Please check your credentials and try again.");
    });


const register = (email: string, password: string) =>
  api
    .post<RegisterResponse>("Auth/register", { Email: email, Password: password })
    .then((res) => {
      const message = res.data.message;
      if (message) {
        alert(message);
        window.location.href = "/login";
      } else {
        throw new Error("Unexpected response from server");
      }
    })
    .catch((error) => {
      console.error("Registration failed:", error);
      alert("Registration failed. Please try again.");
    });

const uploadPortfolio = (file: File) => {
  const formData = new FormData();
  formData.append('File', file);

  return api
    .post("Portfolio/upload", formData)
    .then((res) => res.data)
    .catch((error) => {
      console.error("Upload failed:", error.response || error);
      throw error;
    });
};
      
const getCurrentPortfolio = () =>
  api.get<PortfolioDto>("Portfolio/current").then((res) => res.data);

const updatePortfolio = () => 
   api.patch(`/Portfolio/update-portfolio`);

const requests = {
  login,
  register,
  uploadPortfolio,
  getCurrentPortfolio,
  updatePortfolio,
};

export default requests;
