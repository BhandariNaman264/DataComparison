import React from "react";
import { Container, Box, makeStyles, Grid } from "@material-ui/core";

const useStyles = makeStyles((theme) => ({
  footer: {
    position: "fixed",
    left: 0,
    bottom: 0,
    width: "100%",
    color: "white",
    textAlign: "left",
    backgroundColor: "#616161",
    padding: 10,
    fontSize: 12,
    zIndex: 1201,
  },
}));

const Footer = () => {
  const classes = useStyles();

  function getChromeVersion() {
    var raw = navigator.userAgent.match(/Chrom(e|ium)\/([0-9]+)\./);

    return raw ? parseInt(raw[2], 10) : false;
  }

  let version = getChromeVersion();
  let message;
  let job_status =
    "Refer to About Page for Latest Anouncements related to Background Jobs available in Compare Pay Tool";
  if (version === false) {
    message = "Compare Pay Tool client supported on your Web Browser";
  } else {
    message =
      "Compare Pay Tool client supported on Google Chrome Version " +
      version.toString();
  }

  return (
    <div className={classes.footer}>
      <Container maxWidth={false}>
        <Box>
          <Grid container spacing={3}>
            <Grid item lg={3} md={3} xl={3} xs={6}>
              {message}
            </Grid>
            <Grid item lg={3} md={3} xl={3} xs={6}></Grid>
            <Grid item lg={3} md={3} xl={3} xs={6}></Grid>
            <Grid item lg={3} md={3} xl={3} xs={6}>
              {job_status}
            </Grid>
          </Grid>
        </Box>
      </Container>
    </div>
  );
};

export default Footer;
