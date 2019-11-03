import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import MovieHeadshot from 'Movie/MovieHeadshot';
import styles from './MovieCrewRow.css';

class MovieCrewRow extends Component {

  //
  // Render

  render() {
    const {
      name,
      department,
      job,
      images
    } = this.props;

    return (
      <TableRow>

        <TableRowCell>
          <MovieHeadshot
            className={styles.poster}
            images={images}
            size={20}
            lazy={false}
          />
        </TableRowCell>

        <TableRowCell>
          {name}
        </TableRowCell>

        <TableRowCell>
          {department}
        </TableRowCell>

        <TableRowCell>
          {job}
        </TableRowCell>

      </TableRow>
    );
  }
}

MovieCrewRow.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  department: PropTypes.string.isRequired,
  job: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieCrewRow;
