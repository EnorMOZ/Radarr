import React from 'react';
import MovieCrewTableContent from './MovieCrewTableContent';

function MovieCrewTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <MovieCrewTableContent
      {...otherProps}
    />
  );
}

MovieCrewTable.propTypes = {
};

export default MovieCrewTable;
