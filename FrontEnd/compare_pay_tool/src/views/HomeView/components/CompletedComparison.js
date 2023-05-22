import React, { useState, useEffect } from "react";
import { Bar } from "react-chartjs-2";
import {
  Box,
  Paper,
  Card,
  CardContent,
  Typography,
  Divider,
  useTheme,
  makeStyles,
  colors,
} from "@material-ui/core";

const useStyles = makeStyles((theme) => ({
  note: {
    textAlign: "left",
    fontSize: 13,
    marginTop: 34,
  },
  paper: {
    width: "100%",
    marginBottom: theme.spacing(2),
  },
}));

const CompletedComparison = ({ result }) => {
  const classes = useStyles();
  const theme = useTheme();

  const [dates, setDates] = useState([]);

  const [count, setCount] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    var seen = {};
    var out = [];
    var j = 0;
    result.reverse();
    result.forEach(function (item) {
      var x = new Date(item.date);
      var d = `${x.getDate()} ${x.toLocaleString("default", {
        month: "short",
      })}`;
      if (seen[d]) {
        seen[d] += 1;
      } else {
        seen[d] = 1;
        out[j++] = d;
      }
    });

    let tmp = [];
    out.forEach(function (d) {
      tmp.push(seen[d]);
    });
    setDates(out);
    setCount(tmp);
    setLoading(false);
  }, [result]);

  const data = {
    datasets: [
      {
        backgroundColor: colors.blueGrey[900],
        data: count,
        label: "Comparisons",
        barPercentage: 0.5,
        maxBarThickness: 10,
        barThickness: 12,
        categoryPercentage: 0.5,
      },
    ],
    labels: dates,
  };

  const options = {
    animation: false,
    cornerRadius: 20,
    layout: { padding: 0 },
    legend: { display: false },
    maintainAspectRatio: false,
    responsive: true,
    scales: {
      xAxes: [
        {
          ticks: {
            fontColor: theme.palette.text.secondary,
          },
          gridLines: {
            display: false,
            drawBorder: false,
          },
        },
      ],
      yAxes: [
        {
          ticks: {
            fontColor: theme.palette.text.secondary,
            beginAtZero: true,
            min: 0,
          },
          gridLines: {
            borderDash: [2],
            borderDashOffset: [2],
            color: theme.palette.divider,
            drawBorder: false,
            zeroLineBorderDash: [2],
            zeroLineBorderDashOffset: [2],
            zeroLineColor: theme.palette.divider,
          },
        },
      ],
    },
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
        Number of Comparisons Initiated - {result.length}
      </Typography>
      <Divider />
      <Paper className={classes.paper}>
        <CardContent style={{ paddingBottom: 16 }}>
          <Box height={400} position="relative">
            <Bar data={data} options={options} />
          </Box>
          <Box mt={2}>
            <p className={classes.note}>
              * Graph displays number of comparison initiated on different days
              for last 30 days
            </p>
          </Box>
        </CardContent>
      </Paper>
    </Card>
  );
};

export default CompletedComparison;
