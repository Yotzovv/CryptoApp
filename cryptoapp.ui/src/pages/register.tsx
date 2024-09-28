import { Button, TextField, Paper, Grid, Container, Typography } from "@mui/material";
import React from "react";
import request from "../common/requests";

const Register = () => {
  const [email, setEmail] = React.useState("");
  const [password, setPassword] = React.useState("");

  const onChangeEmail = (e: any) => {
    setEmail(e.target.value);
  };

  const onChangePassword = (e: any) => {
    setPassword(e.target.value);
  };

  const onRegister = () => {
    request.register(email, password)
  };

  return (
    <Container maxWidth="sm">
      <Grid container justifyContent="center" alignItems="center" style={{ minHeight: "70vh" }}>
        <Grid item xs={12} sm={8} md={6} lg={10}>
          <Paper style={{ padding: 24 }} elevation={20}>
            <Typography variant="h4" component="h1" align="center">
              Register
            </Typography>
            <TextField
              onChange={onChangeEmail}
              fullWidth
              margin="normal"
              label="Email"
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
                <Button variant="outlined" href="/login">Cancel</Button>
              </Grid>
              <Grid item>
                <Button variant="contained" color="primary" onClick={onRegister}>
                  Confirm
                </Button>
              </Grid>
            </Grid>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
}

export default Register