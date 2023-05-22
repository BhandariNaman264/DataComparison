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

const ClientCatSelect = ({
  ClientInfo,
  setClientInfo,
  categoryList,
  setSubOpen,
}) => {
  const classes = useStyles();
  if (categoryList) {
    return (
      <Box mb={2}>
        <FormControl required className={classes.formControl}>
          <Autocomplete
            id="clientCategory"
            options={categoryList}
            required
            value={ClientInfo}
            // If the user decides to change the category, all category data previously fetched gets overwritten
            onChange={(event, newValue) => {
              setSubOpen(false);
              setClientInfo((prev) => {
                let update = { ...prev };
                update.category = newValue ? newValue.category : "";
                update.status = newValue ? newValue.status : 0;
                update.clientId = -1;
                update.clientName = "";
                update.clientDb = "";
                update.adminDb = "";
                update.adminDbSrv = "";
                update.namespace = null;
                update.namespace2 = null;
                return update;
              });
            }}
            getOptionSelected={(option, value) =>
              option.category === value.category &&
              option.status === value.status
            }
            getOptionLabel={(option) =>
              option.category ? option.category.toLowerCase() : ""
            }
            renderInput={(params) => (
              <TextField
                {...params}
                label="Client Test Infrastructure"
                required
                variant="outlined"
              />
            )}
          />
        </FormControl>
      </Box>
    );
  }
};

export default ClientCatSelect;
