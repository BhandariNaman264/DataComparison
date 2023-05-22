import React from "react";
import { Container, Grid, makeStyles } from "@material-ui/core";
import Page from "src/components/Page";
import ResultTable from "./components/ResultTable";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.white,
    marginBottom: 35,
    paddingBottom: theme.spacing(100),
    paddingTop: theme.spacing(3),
  },
}));

const ResultsPage = ({ userInfo }) => {
  const classes = useStyles();

  return (
    <Page className={classes.root} title="Results Page">
      <Container maxWidth={false}>
        <Grid container spacing={3}>
          <Grid item lg={12} md={12} xl={12} xs={12}>
            <ResultTable userInfo={userInfo} />
          </Grid>
        </Grid>
      </Container>
    </Page>
  );
};

export default ResultsPage;
