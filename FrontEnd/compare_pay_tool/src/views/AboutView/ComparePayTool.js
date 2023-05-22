import React from "react";
import { Box, Grid, Container, Card } from "@material-ui/core";
import Page from "src/components/Page";
import { makeStyles } from "@material-ui/core";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.dark,
    marginBottom: 35,
    paddingBottom: theme.spacing(72.5),
    paddingTop: theme.spacing(35),
  },
  subheader: {
    margin: "5px",
    fontSize: "16px",
  },
  card: {
    marginTop: theme.spacing(3),
    height: "100%",
    padding: theme.spacing(3),
  },
  icon: {
    fontSize: "3rem",
    color: "#ffcc00",
  },
}));

const ComparePayTool = () => {
  const classes = useStyles();

  return (
    <Page className={classes.root} title="About">
      <Container maxWidth="lg">
        <Grid container spacing={3}>
          <Grid item lg={12} md={12} xl={12} xs={12}>
            <Card className={classes.card}>
              <Box>
                <h2>Compare Pay Tool</h2>
              </Box>
              <Box mt={2}>
                <h4>
                  This tool based on the Input parameters starts Pay Summary
                  Recalc, Base Rate Recalc, Job Step Recalc, Schedule Cost
                  Recalc, Award Entitlement, and Pay Export Background Job,
                  generates Output Files, Compares them and Analyze Record.
                </h4>
              </Box>
              <Box mt={2}>
                <h3>Announcements:</h3>
              </Box>
              <Box mt={2}>
                <h4>
                  For Pay Export - Background Job: Comparison and Analyzing
                  implementation is not completed, Therefore once the Background
                  Job is Finished with JobCompleted status, proceed with Manual
                  Comparison. For more information:
                  <a
                    href="https://wiki.dayforce.com/display/WFM/Export+File+-+Comparison"
                    target="blank"
                  >
                    {" "}
                    https://wiki.dayforce.com/display/WFM/Export+File+-+Comparison{" "}
                  </a>
                </h4>
              </Box>
              <Box mt={2}>
                <h4> For more information: </h4>
                <h4>
                  <a
                    href="https://wiki.dayforce.com/pages/viewpage.action?spaceKey=WFM&title=ComparePay+Tool"
                    target="blank"
                  >
                    {" "}
                    https://wiki.dayforce.com/pages/viewpage.action?spaceKey=WFM&title=ComparePay+Tool{" "}
                  </a>
                </h4>
              </Box>
              <Box mt={3}>
                <h5>The Compare Pay Tool Development Team</h5>
              </Box>
            </Card>
          </Grid>
        </Grid>
      </Container>
    </Page>
  );
};

export default ComparePayTool;
