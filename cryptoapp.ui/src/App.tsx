import { BrowserRouter, Routes, Route } from "react-router-dom";
import Box from "@mui/material/Box";
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import React from "react";
import { PaletteMode } from "@mui/material";
import Login from "./pages/login";
import Register from "./pages/register";
import Home from "./pages/home";

function App() {
  const [mode, setMode] = React.useState<PaletteMode>('dark');
  const darkTheme = createTheme({
    palette: {
      mode: mode,
    },
    components: {
      MuiTypography: {
        styleOverrides: {
          root: {
            color: mode === 'dark' ? 'white' : undefined,
          },
        },
      },
    },
  });
  
  const getComponent = (component: JSX.Element) => {
    const jwt: String = sessionStorage['jwt'];
    if((!jwt || jwt.length < 10)) {
      return <Login />
    }
      return component;
  }
  
  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline />
      <BrowserRouter>
        <Box sx={{ marginLeft: "0%" }}>
          <Routes>
            <Route path="/" element={getComponent(<Home />)} />
            <Route path="/home" element={getComponent(<Home />)} />
            <Route path="/login" element={getComponent(<Login />)} />
            <Route path="/register" element={<Register />} />
          </Routes>
        </Box>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;