import React, { useEffect, useRef, useState } from "react";
import {
  Box,
  Button,
  Typography,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { UploadFile } from "@mui/icons-material";
import requests, { PortfolioDto } from "../common/requests";

const Home: React.FC = () => {
  const theme = useTheme();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState<boolean>(false);
  const [portfolio, setPortfolio] = useState<PortfolioDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [refreshInterval, setRefreshInterval] = useState<number>(5); // Default to 5 minutes
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setError(null);
    if (e.target.files && e.target.files.length > 0) {
      const file = e.target.files[0];
      setSelectedFile(file);
    }
  };

  const fetchPortfolio = async () => {
    try {
      await requests.updatePortfolio();
      const currentPortfolio = await requests.getCurrentPortfolio();
      setPortfolio(currentPortfolio);
    } catch (err: any) {
      setError("An error occurred while fetching the portfolio.");
    }
  };

  useEffect(() => {
    if (portfolio) {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }

      const intervalInMs = refreshInterval * 60 * 1000;
      intervalRef.current = setInterval(fetchPortfolio, intervalInMs);
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [portfolio, refreshInterval]);

  const handleUpload = async () => {
    if (!selectedFile) {
      setError("Please select a file to upload.");
      return;
    }
  
    setUploading(true);
    setError(null);
  
    try {
      await requests.uploadPortfolio(selectedFile);

      const currentPortfolio = await requests.getCurrentPortfolio();
      setPortfolio(currentPortfolio);
    } catch (err: any) {

      if (err.response && err.response.data && err.response.data.message) {
        setError(err.response.data.message);
      } else {
        setError("An error occurred while uploading the file.");
      }
    } finally {
      setUploading(false);
    }
  };

  return (
    <Box
      sx={{
        padding: theme.spacing(4),
        maxWidth: "800px",
        margin: "0 auto",
      }}
    >
      <Typography variant="h4" gutterBottom>
        Welcome to Your Portfolio
      </Typography>
      
      {!portfolio && (
      <Box
        sx={{
          border: `2px dashed ${theme.palette.primary.main}`,
          padding: theme.spacing(4),
          borderRadius: "8px",
          textAlign: "center",
          marginBottom: theme.spacing(4),
        }}
      >
        <UploadFile sx={{ fontSize: 60, color: theme.palette.primary.main }} />
        <Typography variant="h6" gutterBottom>
          Upload Your Coins File
        </Typography>
        <Typography variant="body2" color="textSecondary" gutterBottom>
          Format:
          <br />
          amount|coin|initialBuyPrice
          <br />
          e.g., 10|ETH|123.14
        </Typography>
        <input
          accept=".txt"
          style={{ display: "none" }}
          id="raised-button-file"
          type="file"
          onChange={handleFileChange}
        />
        <label htmlFor="raised-button-file">
          <Button variant="contained" component="span" sx={{ marginTop: theme.spacing(2) }}>
            Choose File
          </Button>
        </label>
        {selectedFile && (
          <Typography variant="body1" sx={{ marginTop: theme.spacing(1) }}>
            Selected File: {selectedFile.name}
          </Typography>
        )}
        <Button
          variant="contained"
          color="primary"
          onClick={handleUpload}
          disabled={uploading || !selectedFile}
          sx={{ marginTop: theme.spacing(2) }}
        >
          {uploading ? <CircularProgress size={24} color="inherit" /> : "Upload"}
        </Button>
        {error && (
          <Alert severity="error" sx={{ marginTop: theme.spacing(2) }}>
            {error}
          </Alert>
        )}
      </Box>
    )}

    {portfolio && (
        <Box>
          <Typography variant="h5" gutterBottom>
            Your Portfolio
          </Typography>
          <Typography variant="body1" gutterBottom>
            Initial Portfolio Value: ${portfolio.initialPortfolioValue.toFixed(2)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            Current Portfolio Value: ${portfolio.currentPortfolioValue.toFixed(2)}
          </Typography>
          <Typography variant="body1" gutterBottom>
            Overall Change: {portfolio.overallChangePercentage.toFixed(2)}%
          </Typography>

            <Box sx={{ marginBottom: theme.spacing(4) }}>
            <Typography variant="h6" gutterBottom>
                Set Refresh Interval (in minutes)
            </Typography>
            <input
                type="number"
                value={refreshInterval}
                onChange={(e) => setRefreshInterval(parseInt(e.target.value))}
                min="1"
                style={{ width: "60px", marginRight: "10px" }}
            />
            <Button variant="contained" onClick={fetchPortfolio}>
                Refresh Now
            </Button>
            </Box>

          {/* Table of Currencies */}
          <TableContainer component={Paper} sx={{ marginTop: theme.spacing(2), marginBottom: theme.spacing(4) }}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Coin</TableCell>
                  <TableCell>Amount</TableCell>
                  <TableCell>Initial Buy Price ($)</TableCell>
                  <TableCell>Current Price ($)</TableCell>
                  <TableCell>Change (%)</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {portfolio.currencies.map((currency) => (
                  <TableRow key={currency.id}>
                    <TableCell>{currency.coin}</TableCell>
                    <TableCell>{currency.amount}</TableCell>
                    <TableCell>{currency.initialBuyPrice.toFixed(2)}</TableCell>
                    <TableCell>{currency.currentPrice.toFixed(5)}</TableCell>
                    <TableCell
                      sx={{
                        color: currency.changePercentage >= 0 ? "green" : "red",
                      }}
                    >
                      {currency.changePercentage.toFixed(2)}%
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
    )}
    </Box>
  );
};

export default Home;