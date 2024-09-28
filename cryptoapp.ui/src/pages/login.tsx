import { useState } from "react";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Grid from "@mui/material/Grid";
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";
import { Container } from "@mui/system";
import requests from '../common/requests'

const Login = () => {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");

  const onChangeUserName = (e: any) => {
    setUserName(e.target.value);
  };

  const onChangePassword = (e: any) => {
    setPassword(e.target.value);
  };

  const onLogin = () => {
    requests.login(userName, password)
  };

  return (
    <Container maxWidth="sm">
      <Grid container justifyContent="center" alignItems="center" style={{ minHeight: "70vh" }}>
        <Grid item xs={12} sm={8} md={6} lg={10}>
          <Paper style={{ padding: 24 }} elevation={20}>
            <Typography variant="h4" component="h1" align="center">
              Login
            </Typography>
            <TextField
              onChange={onChangeUserName}
              fullWidth
              margin="normal"
              label="User Name"
              variant="outlined"
            />
            <TextField
              onChange={onChangePassword}
              fullWidth
              margin="normal"
              label="Password"
              type="password"
              variant="outlined"
            />
            <Grid container justifyContent="flex-end" spacing={2} style={{ marginTop: 24 }}>
              <Grid item>
                <Button variant="outlined" href="/register">Register</Button>
              </Grid>
              <Grid item>
                <Button variant="contained" color="primary" onClick={onLogin}>
                  Login
                </Button>
              </Grid>
            </Grid>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
}

export default Login;