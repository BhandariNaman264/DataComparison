import React from "react";
import { Box, FormControl, TextField, makeStyles } from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";

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

const ForceCompareOnly = ({ ForceCompare, setForceCompare, setSubOpen }) => {
  const classes = useStyles();
  //Option for selecting True or False
  const forceCompareOnly = [
    { booleanValue: "False", val: 0, bool: false },
    { booleanValue: "True", val: 1, bool: true },
  ];
  return (
    <Box mb={2}>
      <FormControl required className={classes.formControl}>
        <Autocomplete
          id="forceCompareOnly"
          options={forceCompareOnly}
          required
          value={ForceCompare}
          getOptionSelected={(option, value) => option.val === value.val}
          getOptionLabel={(option) =>
            option.val === 0 ? "False" : option.val === 1 ? "True" : ""
          }
          onChange={(event, newValue) => {
            setForceCompare((prev) => {
              let update = { ...prev };
              update.booleanValue = newValue ? newValue.booleanValue : "";
              update.val = newValue ? newValue.val : -1;
              update.bool = newValue ? newValue.bool : null;
              return update;
            });

            setSubOpen(false);
          }}
          renderInput={(params) => (
            <TextField
              {...params}
              label="Force Compare Only"
              required
              variant="outlined"
            />
          )}
        />
      </FormControl>
    </Box>
  );
};

export default ForceCompareOnly;
