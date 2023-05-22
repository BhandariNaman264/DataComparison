import React from "react";
import { Box, FormControl, makeStyles } from "@material-ui/core";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import DateMomentUtils from "@date-io/moment";
import { DatePicker, MuiPickersUtilsProvider } from "@material-ui/pickers";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.dark,
    minHeight: "100%",
    paddingBottom: theme.spacing(3),
    paddingTop: theme.spacing(3),
  },
  card: {
    height: "100%",
    color: theme.palette.text.secondary,
    minHeight: 688,
  },
  formControl: {
    margin: theme.spacing(1),
    minWidth: 450,
    maxWidth: 450,
  },
  sortFormControl: {
    maxWidth: 450,
    minWidth: 450,
  },
  sortGroup: {
    position: "absolute",
    right: 0,
    top: -40,
  },
  server: {
    wordWrap: "word-break",
    textAlign: "left",
    color: "rgba(0, 0, 0, 0.54)",
  },
  serverHead: {
    color: "rgba(0, 0, 0, 0.83)",
    textAlign: "left",
    marginTop: 8,
    marginBottom: 3,
  },
}));

const TimeEnd = ({ End, setEnd, setSubOpen, Start }) => {
  const classes = useStyles();

  return (
    <Box mb={2}>
      <FormControl required className={classes.formControl}>
        <MuiPickersUtilsProvider utils={DateMomentUtils}>
          <DatePicker
            format="yyyy-MM-DD"
            value={End}
            selected={End ? End : null}
            onChange={(e) => {
              setEnd(e);
              setSubOpen(false);
            }}
            disabled={Start ? false : true}
            minDate={Start}
            minDateMessage="End Date must be after Start Date, to initiate comparison"
            label="End Date"
            required
          />
        </MuiPickersUtilsProvider>
      </FormControl>
    </Box>
  );
};

export default TimeEnd;
