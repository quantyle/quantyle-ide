import React from 'react';
import PropTypes from 'prop-types';
import clsx from 'clsx';
import {
  Modal,
  withStyles,
} from '@material-ui/core';
import Button from '../button/Button';
import modalStyle from "../../variables/styles/modalStyle";

const FileModal = ({ ...props }) => {
  const {
    classes,
    open,
    onClose,
    onFileClick,
    files,
  } = props;

  return (
    <Modal
      open={open}
      onClose={onClose}
      aria-labelledby="simple-modal-title"
      aria-describedby="simple-modal-description"
    >
      <div className={clsx(classes.paper)}>
        {Object.keys(files).map((item, index) =>
          <div key={index} className={classes.button}>
            <Button primary onClick={() => onFileClick(files[item])}>
              {item}
            </Button>
          </div>
        )}
      </div>
    </Modal>

  );
}

FileModal.propTypes = {
  className: PropTypes.string,
  message: PropTypes.string,
  onClose: PropTypes.func,
};

export default withStyles(modalStyle)(FileModal);
