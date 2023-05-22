import React, { useEffect, useState } from "react";
import { Doughnut } from "react-chartjs-2";
import {
  Box,
  Paper,
  Card,
  CardContent,
  Divider,
  Typography,
  colors,
  makeStyles,
  useTheme,
} from "@material-ui/core";
import CloseIcon from "@material-ui/icons/Close";
import DoneIcon from "@material-ui/icons/Done";
import DirectionsRunIcon from "@material-ui/icons/DirectionsRun";
import ErrorIcon from "@material-ui/icons/Error";
import HelpOutlineIcon from "@material-ui/icons/HelpOutline";

const useStyles = makeStyles((theme) => ({
  note: {
    textAlign: "left",
    fontSize: 13,
  },
  paper: {
    width: "100%",
    marginBottom: theme.spacing(2),
  },
}));

const Statistics = ({ result }) => {
  const classes = useStyles();
  const theme = useTheme();
  const [differenceCount, setdifferenceCount] = useState(0);
  const [nodifferenceCount, setnodifferenceCount] = useState(0);
  const [runningCount, setrunningCount] = useState(0);
  const [failedCount, setfailedCount] = useState(0);
  const [noresultsCount, setnoresultsCount] = useState(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    var set = {};

    result.forEach((item) => {
      if (item.results === "WARNING") {
        if (set["Difference"]) {
          set["Difference"] += 1;
        } else {
          set["Difference"] = 1;
        }
      } else if (item.results === "SUCCESS") {
        if (set["No Difference"]) {
          set["No Difference"] += 1;
        } else {
          set["No Difference"] = 1;
        }
      } else if (item.compared === 1) {
        if (set["In Running"]) {
          set["In Running"] += 1;
        } else {
          set["In Running"] = 1;
        }
      } else if (item.compared === 3) {
        if (set["Failed"]) {
          set["Failed"] += 1;
        } else {
          set["Failed"] = 1;
        }
      } else if (item.compared === 0) {
        if (set["No Results"]) {
          set["No Results"] += 1;
        } else {
          set["No Results"] = 1;
        }
      }
    });

    setdifferenceCount(set["Difference"] ?? 0);
    setnodifferenceCount(set["No Difference"] ?? 0);
    setrunningCount(set["In Running"] ?? 0);
    setfailedCount(set["Failed"] ?? 0);
    setnoresultsCount(set["No Results"] ?? 0);
    setLoading(false);
  }, [result]);

  const options = {
    animation: false,
    cutoutPercentage: 80,
    layout: { padding: 0 },
    legend: {
      display: false,
    },
    maintainAspectRatio: false,
    responsive: true,
    tooltips: {
      backgroundColor: theme.palette.background.default,
      bodyFontColor: theme.palette.text.secondary,
      borderColor: theme.palette.divider,
      borderWidth: 1,
      enabled: true,
      footerFontColor: theme.palette.text.secondary,
      intersect: false,
      mode: "index",
      titleFontColor: theme.palette.text.primary,
    },
  };

  const data = {
    datasets: [
      {
        data: [
          differenceCount,
          nodifferenceCount,
          runningCount,
          failedCount,
          noresultsCount,
        ],
        backgroundColor: [
          "#FF0000",
          "#00FF00",
          "#eec300",
          "#ffcccb",
          "#D6D5CB",
        ],
        borderWidth: 8,
        borderColor: colors.common.white,
        hoverBorderColor: colors.common.white,
      },
    ],
    labels: [
      "Difference",
      "No Difference",
      "In Running",
      "Failed",
      "No Results",
    ],
  };

  const statuses = [
    {
      title: "Difference",
      value: isNaN(parseInt((differenceCount / result.length) * 100))
        ? 0
        : parseInt((differenceCount / result.length) * 100),
      icon: CloseIcon,
      color: "#FF0000",
    },
    {
      title: "No Difference",
      value: isNaN(parseInt((nodifferenceCount / result.length) * 100))
        ? 0
        : parseInt((nodifferenceCount / result.length) * 100),
      icon: DoneIcon,
      color: "#00FF00",
    },
    {
      title: "In Running",
      value: isNaN(parseInt((runningCount / result.length) * 100))
        ? 0
        : parseInt((runningCount / result.length) * 100),
      icon: DirectionsRunIcon,
      color: "#eec300",
    },
    {
      title: "Failed",
      value: isNaN(parseInt((failedCount / result.length) * 100))
        ? 0
        : parseInt((failedCount / result.length) * 100),
      icon: ErrorIcon,
      color: "#ffcccb",
    },
    {
      title: "No Results",
      value: isNaN(parseInt((noresultsCount / result.length) * 100))
        ? 0
        : parseInt((noresultsCount / result.length) * 100),
      icon: HelpOutlineIcon,
      color: "#D6D5CB",
    },
  ];
  return loading ? (
    <></>
  ) : (
    <Card>
      <Typography
        variant="h2"
        align="center"
        color="primary"
        style={{ margin: "2rem" }}
      >
        Comparison Statistics
      </Typography>
      <Divider />
      <Paper className={classes.paper}>
        <CardContent style={{ paddingBottom: 16 }}>
          <Box height={300} position="relative">
            <Doughnut data={data} options={options} />
          </Box>
          <Box display="flex" justifyContent="center" mt={2}>
            {statuses.map(({ color, icon: Icon, title, value }) => (
              <Box key={title} p={1} textAlign="center">
                <Icon color="action" />
                <Typography color="textPrimary" variant="body1">
                  {title}
                </Typography>
                <Typography style={{ color }} variant="h2">
                  {value}%
                </Typography>
              </Box>
            ))}
          </Box>
          <Box mt={2}>
            <p className={classes.note}>
              * Statistics and Analysis of all Comparisons in last 30 days
            </p>
          </Box>
        </CardContent>
      </Paper>
    </Card>
  );
};

export default Statistics;
