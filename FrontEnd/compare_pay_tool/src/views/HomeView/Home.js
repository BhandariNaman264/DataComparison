import React, { useEffect, useState } from "react";
import { Container, Grid, makeStyles } from "@material-ui/core";
import Page from "src/components/Page";
import axios from "axios";
import { inputAPI } from "src/components/APIBase/BaseURL";
import Statistics from "./components/Statistics";
import CompletedComparison from "./components/CompletedComparison";
import JobStatistics from "./components/JobStatistics";
import MyComparisonTable from "./components/MyComparisonTable";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.white,
    marginBottom: 35,
    paddingBottom: theme.spacing(100),
    paddingTop: theme.spacing(3),
  },
}));

const Home = ({ userInfo }) => {
  const classes = useStyles();

  const [result, setresult] = useState([]);

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let resultApi = async () => {
      let url = inputAPI;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setLoading(false);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    resultApi();
  }, []);

  return (
    <Page className={classes.root} title="Compare Pay Tool">
      <Container maxWidth={false}>
        <Grid container spacing={3}>
          <Grid item lg={12} md={12} xl={12} xs={12}>
            <MyComparisonTable
              userInfo={userInfo}
              result={result}
              loading={loading}
            />
          </Grid>
          <Grid item lg={6} md={6} xl={6} xs={6}>
            <Statistics result={result} />
          </Grid>
          <Grid item lg={6} md={6} xl={6} xs={6}>
            <JobStatistics result={result} />
          </Grid>
          <Grid item lg={12} md={12} xl={12} xs={12}>
            <CompletedComparison result={result} />
          </Grid>
        </Grid>
      </Container>
    </Page>
  );
};

export default Home;
