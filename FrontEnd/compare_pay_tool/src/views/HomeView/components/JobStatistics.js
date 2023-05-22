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
import AccountBalanceIcon from "@material-ui/icons/AccountBalance";
import StorageIcon from "@material-ui/icons/Storage";
import FlightTakeoffIcon from "@material-ui/icons/FlightTakeoff";
import WorkIcon from "@material-ui/icons/Work";
import PaymentIcon from "@material-ui/icons/Payment";
import AttachMoneyIcon from "@material-ui/icons/AttachMoney";

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

const JobStatistics = ({ result }) => {
  const classes = useStyles();
  const theme = useTheme();
  const [aeCount, setaeCount] = useState(0);
  const [brrCount, setbrrCount] = useState(0);
  const [exportCount, setexportCount] = useState(0);
  const [jsrCount, setjsrCount] = useState(0);
  const [psrCount, setpsrCount] = useState(0);
  const [scrCount, setscrCount] = useState(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    var set = {};

    result.forEach((item) => {
      if (item.job === "AE_Sample") {
        if (set["AE"]) {
          set["AE"] += 1;
        } else {
          set["AE"] = 1;
        }
      } else if (item.job === "BRR") {
        if (set["BRR"]) {
          set["BRR"] += 1;
        } else {
          set["BRR"] = 1;
        }
      } else if (item.job === "Export") {
        if (set["Export"]) {
          set["Export"] += 1;
        } else {
          set["Export"] = 1;
        }
      } else if (item.job === "JobStepRecalc") {
        if (set["JSR"]) {
          set["JSR"] += 1;
        } else {
          set["JSR"] = 1;
        }
      } else if (item.job === "PSR") {
        if (set["PSR"]) {
          set["PSR"] += 1;
        } else {
          set["PSR"] = 1;
        }
      } else if (item.job === "SCR") {
        if (set["SCR"]) {
          set["SCR"] += 1;
        } else {
          set["SCR"] = 1;
        }
      }
    });

    setaeCount(set["AE"] ?? 0);
    setbrrCount(set["BRR"] ?? 0);
    setexportCount(set["Export"] ?? 0);
    setjsrCount(set["JSR"] ?? 0);
    setpsrCount(set["PSR"] ?? 0);
    setscrCount(set["SCR"] ?? 0);
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
        data: [aeCount, brrCount, exportCount, jsrCount, psrCount, scrCount],
        backgroundColor: [
          "#8F00FF",
          "#4B0082",
          "#0000FF",
          "#00FF00",
          "#964B00",
          "#FFA500",
        ],
        borderWidth: 8,
        borderColor: colors.common.white,
        hoverBorderColor: colors.common.white,
      },
    ],
    labels: [
      "Award Entitlement",
      "Base Rate Recalc",
      "Pay Export",
      "Job Step Recalc",
      "Pay Summary Recalc",
      "Schedule Cost Recalc",
    ],
  };

  const statuses = [
    {
      title: "Award Entitlement",
      value: isNaN(parseInt((aeCount / result.length) * 100))
        ? 0
        : parseInt((aeCount / result.length) * 100),
      icon: AccountBalanceIcon,
      color: "#8F00FF",
    },
    {
      title: "Base Rate Recalc",
      value: isNaN(parseInt((brrCount / result.length) * 100))
        ? 0
        : parseInt((brrCount / result.length) * 100),
      icon: StorageIcon,
      color: "#4B0082",
    },
    {
      title: "Pay Export",
      value: isNaN(parseInt((exportCount / result.length) * 100))
        ? 0
        : parseInt((exportCount / result.length) * 100),
      icon: FlightTakeoffIcon,
      color: "#0000FF",
    },
    {
      title: "Job Step Recalc",
      value: isNaN(parseInt((jsrCount / result.length) * 100))
        ? 0
        : parseInt((jsrCount / result.length) * 100),
      icon: WorkIcon,
      color: "#00FF00",
    },
    {
      title: "Pay Summary Recalc",
      value: isNaN(parseInt((psrCount / result.length) * 100))
        ? 0
        : parseInt((psrCount / result.length) * 100),
      icon: PaymentIcon,
      color: "#964B00",
    },
    {
      title: "Schedule Cost Recalc",
      value: isNaN(parseInt((scrCount / result.length) * 100))
        ? 0
        : parseInt((scrCount / result.length) * 100),
      icon: AttachMoneyIcon,
      color: "#FFA500",
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
        Job Statistics
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
              * Number of different Job Comparison initiated in last 30 days
            </p>
          </Box>
        </CardContent>
      </Paper>
    </Card>
  );
};

export default JobStatistics;
